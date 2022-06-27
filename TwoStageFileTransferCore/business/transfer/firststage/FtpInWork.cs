using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AryxDevLibrary.utils;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.exceptions;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferCore.utils.events;
using AFileUtils = AryxDevLibrary.utils.FileUtils;
using FileUtils = TwoStageFileTransferCore.utils.FileUtils;

namespace TwoStageFileTransferCore.business.transfer.firststage
{
    public class FtpInWork : AbstractInWork
    {

        protected byte[] Buffer;

        protected readonly IConnexion Connexion;

        public bool IsCanceled { get; set; }

        public FtpInWork(IConnexion connexion, InWorkOptions inWorkOptions) : base(inWorkOptions)
        {
            Connexion = connexion;


        }

        public override void DoTransfert(IProgressTransfer reporter)
        {
            _log.Info("Using FTP :");

            ISet<AppFileFtp> listFiles = new HashSet<AppFileFtp>();

            long partFileMaxLenght = CalculatePartFileMaxLenght();

            int fileCreatedIndex = InWorkOptions.StartPart;




            MainTestFilesNotAlreadyExist(InWorkOptions.Source, InWorkOptions.Target, partFileMaxLenght, !InWorkOptions.CanOverwrite);


            Uri targetUri = InWorkOptions.Target.ToUri();
            bool targetExist = true;
            if (!Connexion.IsDirectoryExists(targetUri))
            {
                targetExist = Connexion.CreateDirectory(targetUri, true);
            }
            if (!targetExist)
            {
                throw new DirectoryNotFoundException(
                    $"Directory '{InWorkOptions.Target}' not found and impossible to create.");
            }


            reporter.OnProgress("Calcul de l'empreinte");
            string sha1 = FileUtils.CalculculateSourceSha1(InWorkOptions.Source);
            _log.Info($"Passphrase for TSFT file: {InWorkOptions.TsftPassphrase}.");
            ConsoleUtils.WriteLineColor($"Passphrase for TSFT file: <*cyan*>{InWorkOptions.TsftPassphrase}<*/*>");


            Console.Write("Creating part files... ");
            DateTime mainStart = DateTime.Now;

            IsCanceled = reporter.CheckIsCanceled();

            reporter.Init();
            using (FileStream fs = new FileStream(InWorkOptions.Source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, InWorkOptions.BufferSize, FileOptions.SequentialScan))
            {
                TotalBytesToRead = fs.Length;
                Buffer = new byte[InWorkOptions.BufferSize];

                // Reprise
                TotalBytesRead = InWorkOptions.StartPart * partFileMaxLenght;
                fs.Seek(TotalBytesRead, SeekOrigin.Begin);
                if (TotalBytesRead > 0)
                {
                    reporter.OnProgress("Resuming", (double)TotalBytesRead / fs.Length,
                        BckgerReportType.ProgressPbarText);
                }


                bool isFirstFileCreated = true;
                while (TotalBytesRead < fs.Length && !IsCanceled)
                {
                    string filenameOut = FileUtils.GetFileName(InWorkOptions.Source.Name, fs.Length, fileCreatedIndex);
                    AppFileFtp fileFtp = new AppFileFtp(InWorkOptions.Target, filenameOut);


                    DateTime localStart = DateTime.Now;

                    long localBytesRead = WritePartFile(partFileMaxLenght, reporter, fs, fileFtp, localStart, 3);


                    listFiles.Add(fileFtp);
                    _log.Debug("> OK");



                    if (isFirstFileCreated)
                    {

                        TsftFileSecured tsftContent = GetTransferExchangeFileContent(InWorkOptions.Source.Name, InWorkOptions.Source.Length, partFileMaxLenght, sha1, InWorkOptions.TsftPassphrase,
                            IncludeMoreThingsInTsftFile);
                        if (tsftContent != null)
                        {
                            InWorkOptions.TsftFilePath = Path.Combine(InWorkOptions.Source.Directory.FullName, InWorkOptions.Source.Name + ".tsft");
                            File.WriteAllText(InWorkOptions.TsftFilePath, tsftContent.SecureContent, Encoding.UTF8);

                            Connexion.UploadFileToServer(fileFtp.DirectoryParent.AbsolutePath,
                                new FileInfo(InWorkOptions.TsftFilePath));

                            reporter.OnTsftFileCreated(
                                new TsftFileCreatedArgs() { Filepath = InWorkOptions.TsftFilePath, Passphrase = tsftContent.PassPhrase });

                        }
                    }

                    if (TotalBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                    {
                        WaitForFreeSpace(listFiles, reporter);
                    }

                    LastPartDone = fileCreatedIndex;
                    fileCreatedIndex++;
                    isFirstFileCreated = false;

                    IsCanceled = reporter.CheckIsCanceled();
                }
            }
            reporter.Dispose();

            if (IsCanceled)
            {
                UserCancelArgs userCancelArgs = new UserCancelArgs
                {
                    FileTransfered = listFiles,
                    TotalBytesToRead = TotalBytesToRead,
                    TotalByteRead = TotalBytesRead,
                };
                throw new UserCancelException(userCancelArgs);
            }

            Console.WriteLine("Done.");
            TimeSpan duration = DateTime.Now - mainStart;
            _log.Info("> Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));
        }




        protected override void IncludeMoreThingsInTsftFile(TsftFile file)
        {
            file.TempDir.Type = TransferTypes.FTP;
            file.TempDir.RemotePort = InWorkOptions.AppArgs.RemotePort;

            if (InWorkOptions.AppArgs.IncludeCredsInTsft)
            {
                file.TempDir.FtpUsername = Connexion.Credentials.UserName;
                file.TempDir.FtpPassword = Connexion.Credentials.Password;
            }

        }

        protected override long CalculatePartFileMaxLenght()
        {
            long partFileMaxLenght = InWorkOptions.PartFileSize;
            if (partFileMaxLenght == -1)
            {
                partFileMaxLenght = Math.Min((int)(InWorkOptions.MaxSizeUsedOnShared / 10), 50 * 1024 * 1024);
            }

            partFileMaxLenght = new[]
            {
                InWorkOptions.MaxSizeUsedOnShared, partFileMaxLenght, InWorkOptions.Source.Length
            }.Min();

            LogUtils.I(_log, $"Part file size: {AFileUtils.HumanReadableSize(partFileMaxLenght)}");
            return partFileMaxLenght;
        }

        private long WritePartFile(long partFileMaxLenght, IProgressTransfer reporter, FileStream fs, AppFileFtp fileOutPath, DateTime localStart, int nbTentative)
        {
            while (nbTentative > 0)
            {
                try
                {
                    return WritePartFile(partFileMaxLenght, reporter, fs, fileOutPath, localStart);

                }
                catch (Exception e)
                {
                    nbTentative--;
                    if (nbTentative <= 0)
                    {
                        throw new CommonAppException("Error while writing part file", e, CommonAppExceptReason.ErrorInStageWritingPartFile);
                    }

                    _log.Info("Error while writing part file. Re-try in 30s");
                    Thread.Sleep(30 * 1000);
                    reporter.OnProgress("Retry", (double)TotalBytesRead / fs.Length,
                        BckgerReportType.ProgressPbarText);
                }

            }
            return 0;
        }


        protected virtual long WritePartFile(long chunk, IProgressTransfer reporter, FileStream fs, AppFileFtp fileOutPath, DateTime localStart)
        {
            long localBytesRead = 0;

            try
            {


                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileOutPath.FileTemp);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = Connexion.Credentials;
                request.KeepAlive = true;
                request.UsePassive = true;
                request.UseBinary = true;


                using (Stream fo = request.GetRequestStream())
                {


                    string msg = "Creating part file " + fileOutPath.FileTemp.LocalPath;
                    reporter.OnProgress($"TSFT - In - {msg}");
                    _log.Debug(msg);

                    Array.Clear(Buffer, 0, Buffer.Length);
                    int bytesRead;

                    while ((bytesRead = fs.Read(Buffer, 0, Buffer.Length)) > 0)
                    {
                        TotalBytesRead += bytesRead;
                        localBytesRead += bytesRead;

                        fo.Write(Buffer, 0, bytesRead);
                        reporter.OnProgress(
                            CommonAppUtils.GetTransferSpeed(localBytesRead, localStart),
                            (double)TotalBytesRead / fs.Length,
                            BckgerReportType.ProgressPbarText);


                        if (localBytesRead + InWorkOptions.BufferSize > chunk ||
                            localBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                        {
                            break;
                        }
                    }

                    fileOutPath.Length = localBytesRead;

                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    _log.Debug($"Upload File Complete, status {response.StatusDescription}");
                    if (response.StatusCode != FtpStatusCode.ClosingData)
                    {
                        throw new Exception($"Error during transfer of {fileOutPath.File.LocalPath}");
                    }
                }


                fileOutPath.MoveToNormal(Connexion);
            }
            catch (Exception ex)
            {
                _log.Debug($"Error while uploading : {ex.Message}");

                if (fileOutPath.Exists(Connexion))
                {
                    fileOutPath.Delete(Connexion);
                }

                if (fileOutPath.Exists(Connexion, true))
                {
                    fileOutPath.Delete(Connexion, true);
                }

                throw ex;
            }

            return localBytesRead;
        }


        private void WaitForFreeSpace(ISet<AppFileFtp> listFiles, IProgressTransfer transferReporter)
        {
            bool mustWriteLogStatus = true;
            long filesSize = listFiles.Where(r => r.Exists(Connexion))
                .Sum(f => f.Length);

            TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
            while (filesSize + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared && !IsCanceled)
            {
                HashSet<AppFileFtp> setFilesExist = new HashSet<AppFileFtp>(listFiles.Where(r => r.Exists(Connexion)).ToList());

                filesSize = setFilesExist.Sum(f => f.Length);

                transferReporter.OnProgress("TSFT - In - Waiting for OUT mode to work and freeing disk space...");

                if (mustWriteLogStatus)
                {
                    _log.Debug("Waiting for OUT mode to work and freeing disk space : {0} + {1} > {2}", filesSize, InWorkOptions.BufferSize, InWorkOptions.MaxSizeUsedOnShared);
                    mustWriteLogStatus = false;
                }

                Thread.Sleep(250);
                listFiles = setFilesExist;
                Thread.Sleep(250);

                if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(InWorkOptions.NbMinWaitForFreeSpace))
                {
                    throw new Exception("Waits too long time for disk space");
                }

            }
        }

        protected override void TestFilesNotAlreadyExist(FileInfo source, string target, long chunkSize, bool exceptionIfExists = false)
        {
            long nbFiles = (source.Length / chunkSize) + (source.Length % chunkSize == 0 ? 0 : 1);
            for (long i = 0; i < nbFiles; i++)
            {
                Uri tmpFile = FtpUtils.FtpPathCombine(target, "~" + FileUtils.GetFileName(InWorkOptions.Source.Name, source.Length, i)).ToUri();
                if (Connexion.IsFileExists(tmpFile))
                {
                    _log.Debug($"File {tmpFile.AbsoluteUri} already exists.");
                    Connexion.DeleteFile(tmpFile.AbsolutePath);
                }

                Uri realPartFile = FtpUtils.FtpPathCombine(target, FileUtils.GetFileName(InWorkOptions.Source.Name, source.Length, i)).ToUri();
                if (Connexion.IsFileExists(realPartFile))
                {
                    if (exceptionIfExists)
                    {
                        throw new IOException($"File {realPartFile.AbsoluteUri} already exists.");
                    }

                    _log.Warn("File {0} already exists.", realPartFile);
                    Connexion.DeleteFile(realPartFile);
                }
            }
        }
    }
}
