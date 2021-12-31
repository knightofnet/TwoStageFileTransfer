using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.business.connexions;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.exceptions;
using TwoStageFileTransfer.utils;
using FileUtils = TwoStageFileTransfer.utils.FileUtils;
using AFileUtils = AryxDevLibrary.utils.FileUtils;

namespace TwoStageFileTransfer.business.transferworkers.inwork
{
    class FtpInWork : AbstractInWork
    {
        private long _totalBytesRead ;
        private byte[] _buffer;

        private readonly IConnexion _connexion;


        public FtpInWork(IConnexion connexion, InWorkOptions inWorkOptions ) : base(inWorkOptions)
        {
            _connexion = connexion;
        }

        public override void DoTransfert()
        {
            long partFileMaxLenght = CalculatePartFileMaxLenght();

            int fileCreatedIndex = InWorkOptions.StartPart;

            HashSet<AppFileFtp> listFiles = new HashSet<AppFileFtp>();


            MainTestFilesNotAlreadyExist(InWorkOptions.Source, InWorkOptions.Target, partFileMaxLenght, !InWorkOptions.CanOverwrite);

            Uri targetUri = UriUtils.NewFtpUri(InWorkOptions.Target);
            bool targetExist = true;
            if (!_connexion.IsDirectoryExists(targetUri.AbsolutePath))
            {
                targetExist = _connexion.CreateDirectory(targetUri.AbsolutePath, true); 
            }
            if (!targetExist)
            {
                throw new DirectoryNotFoundException(
                    $"Directory '{InWorkOptions.Target}' not found and impossible to create.");
            }


            string sha1 = FileUtils.CalculculateSourceSha1(InWorkOptions.Source);


            Console.Write("Creating part files... ");
            DateTime mainStart = DateTime.Now;
            using (ProgressBar pbar = new ProgressBar())
            {
                using (FileStream fs = new FileStream(InWorkOptions.Source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, InWorkOptions.BufferSize, FileOptions.SequentialScan))
                {
                    _buffer = new byte[InWorkOptions.BufferSize];

                    _totalBytesRead = InWorkOptions.StartPart * partFileMaxLenght;
                    fs.Seek(_totalBytesRead, SeekOrigin.Begin);
                    if (_totalBytesRead > 0)
                    {
                        pbar.Report((double)_totalBytesRead / fs.Length, "Resuming");
                    }

                    bool isFirstFileCreated = true;
                    while (_totalBytesRead < fs.Length)
                    {
                        string filenameOut = FileUtils.GetFileName(InWorkOptions.Source.Name, fs.Length, fileCreatedIndex);
                        AppFileFtp fileFtp = new AppFileFtp(InWorkOptions.Target, filenameOut);



                        DateTime localStart = DateTime.Now;

                        long localBytesRead = WritePartFile(partFileMaxLenght, pbar, fs, fileFtp, localStart,3);


                        listFiles.Add(fileFtp);
                        _log.Debug("> OK");

                        if (_totalBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                        {
                            WaitForFreeSpace(listFiles);
                        }

                        if (isFirstFileCreated)
                        {

                            string tsftContent = WriteTransferExchangeFile(InWorkOptions.Source.Name, InWorkOptions.Source.Length, partFileMaxLenght, sha1,
                                file =>
                                {
                                    file.TempDir.Type = TransferTypes.FTP;

                                    if (InWorkOptions.AppArgs.IncludeCredsInTsft)
                                    {
                                        file.TempDir.FtpUsername = _connexion.Credentials.UserName;
                                        file.TempDir.FtpPassword = _connexion.Credentials.Password;
                                    }
                                });
                            if (tsftContent != null)
                            {
                                InWorkOptions.TsftFilePath = Path.Combine(InWorkOptions.Source.Directory.FullName, InWorkOptions.Source.Name + ".tsft");
                                File.WriteAllText(InWorkOptions.TsftFilePath, tsftContent, Encoding.UTF8);

                                _connexion.UploadFileToServer(fileFtp.DirectoryParent.AbsolutePath,
                                    new FileInfo(InWorkOptions.TsftFilePath));

                                
                            }
                        }

                        LastPartDone = fileCreatedIndex;
                        fileCreatedIndex++;
                        isFirstFileCreated = false;
                    }
                }
            }
            Console.WriteLine("Done.");
            TimeSpan duration = DateTime.Now - mainStart;
            _log.Info("> Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));
        }

        protected override long CalculatePartFileMaxLenght()
        {
            if (InWorkOptions.MaxSizeUsedOnShared == -1)
            {
                InWorkOptions.MaxSizeUsedOnShared = (long)AFileUtils.HumanReadableSizeToLong("20Mo");
            }
            long partFileMaxLenght = Math.Min(InWorkOptions.MaxSizeUsedOnShared / 10, 50 * 1024 * 1024);


            partFileMaxLenght = new[]
            {
                InWorkOptions.MaxSizeUsedOnShared, partFileMaxLenght, InWorkOptions.Source.Length
            }.Min();

            LogUtils.I(_log, $"Part file size: {AryxDevLibrary.utils.FileUtils.HumanReadableSize(partFileMaxLenght)}");
            return partFileMaxLenght;
        }

        private long WritePartFile(long partFileMaxLenght, ProgressBar pbar, FileStream fs, AppFileFtp fileOutPath, DateTime localStart, int nbTentative)
        {

            while (nbTentative > 0)
            {
                try
                {
                    return WritePartFile(partFileMaxLenght, pbar, fs, fileOutPath, localStart);

                }
                catch (Exception e)
                {
                    nbTentative--;
                    if (nbTentative <= 0)
                    {
                        throw new AppException("Error while writing part file", e, EnumExitCodes.KO_WRITING_PARTFILE);
                    }

                    _log.Info("Error while writing part file. Re-try in 30s");
                    Thread.Sleep(30 * 1000);
                    pbar.Report((double)_totalBytesRead / fs.Length, "Retry");
                }

            }
            return 0;
        }


        protected virtual long WritePartFile(long chunk, ProgressBar pbar, FileStream fs, AppFileFtp fileOutPath, DateTime localStart)
        {
            long localBytesRead = 0;

            try
            {
                

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileOutPath.FileTemp);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = _connexion.Credentials;
                request.KeepAlive = true;
                request.UsePassive = true;
                request.UseBinary = true;

                using (Stream fo = request.GetRequestStream())
                {
                  

                    string msg = "Creating part file " + fileOutPath.FileTemp.LocalPath;
                    Console.Title = $"TSFT - In - {msg}";
                    _log.Debug(msg);

                    Array.Clear(_buffer, 0, _buffer.Length);
                    int bytesRead;

                    while ((bytesRead = fs.Read(_buffer, 0, _buffer.Length)) > 0)
                    {
                        _totalBytesRead += bytesRead;
                        localBytesRead += bytesRead;

                        fo.Write(_buffer, 0, bytesRead);
                        pbar.Report((double)_totalBytesRead / fs.Length, AppUtils.GetTransferSpeed(localBytesRead, localStart));


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


                fileOutPath.MoveToNormal(_connexion);
            }
            catch (Exception ex)
            {
                _log.Debug($"Error while uploading : {ex.Message}");
               
                if (fileOutPath.Exists(_connexion))
                {
                    fileOutPath.Delete(_connexion);
                    
                }

                throw ex;
            }

            return localBytesRead;
        }

        private static string GetTransferSpeed(long localBytesRead, DateTime timeStart)
        {
            long diffTime = (long)(DateTime.Now - timeStart).TotalSeconds;
            if (diffTime == 0) return string.Empty;

            return "~" + AFileUtils.HumanReadableSize(localBytesRead / diffTime) + "/s [last part]";
        }

        private void WaitForFreeSpace(HashSet<AppFileFtp> listFiles)
        {
            bool mustWriteLogStatus = true;
            long filesSize = listFiles.Where(r => r.Exists(_connexion))
                .Sum(f => f.Length);

            TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
            while (filesSize + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
            {
                HashSet<AppFileFtp> setFilesExist = new HashSet<AppFileFtp>(listFiles.Where(r => r.Exists(_connexion)).ToList());

                filesSize = setFilesExist.Sum(f => f.Length);

                Console.Title = $"TSFT - In - Waiting for OUT mode to work and freeing disk space...";
                if (mustWriteLogStatus)
                {
                    _log.Info("Waiting for OUT mode to work and freeing disk space : {0} + {1} > {2}", filesSize, InWorkOptions.BufferSize, InWorkOptions.MaxSizeUsedOnShared);
                    mustWriteLogStatus = false;
                }

                Thread.Sleep(250);
                listFiles = setFilesExist;
                Thread.Sleep(250);

                if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
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
                Uri tmpFile = UriUtils.NewFtpUri(FtpUtils.FtpPathCombine(target, "~" + FileUtils.GetFileName(InWorkOptions.Source.Name, source.Length, i)));
                if ( _connexion.IsFileExists(tmpFile.AbsolutePath))
                {
                    _log.Debug($"File {tmpFile.AbsoluteUri} already exists." );
                    _connexion.DeleteFile(tmpFile.AbsolutePath);
                }

                Uri realPartFile = UriUtils.NewFtpUri(FtpUtils.FtpPathCombine(target, FileUtils.GetFileName(InWorkOptions.Source.Name, source.Length, i)));
                if (_connexion.IsFileExists(realPartFile.AbsolutePath))
                {
                    if (exceptionIfExists)
                    {
                        throw new IOException($"File {realPartFile.AbsoluteUri} already exists.");
                    }

                    _log.Warn("File {0} already exists.", realPartFile);
                    _connexion.DeleteFile(realPartFile.AbsolutePath);
                }
            }
        }
    }
}
