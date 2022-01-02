using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.exceptions;
using TwoStageFileTransfer.utils;
using AFileUtils = AryxDevLibrary.utils.FileUtils;

namespace TwoStageFileTransfer.business.transferworkers.inwork
{
    class InToOutWork : AbstractInWork
    {


        //private readonly long _maxTransferFile;
        //private readonly long _chunkSize;

        private long _totalBytesRead ;
        private byte[] _buffer;



        public InToOutWork(InWorkOptions inWorkOptions) : base(inWorkOptions)
        {

        }

        public override void DoTransfert()
        {
            long partFileMaxLenght = CalculatePartFileMaxLenght();

            int fileCreatedIndex = InWorkOptions.StartPart;

            HashSet<FileInfo> listFiles = new HashSet<FileInfo>();

            bool targetExist = true;
            if (!Directory.Exists(InWorkOptions.Target))
            {
                Directory.CreateDirectory(InWorkOptions.Target);
                targetExist = Directory.Exists(InWorkOptions.Target);
            }
            if (!targetExist)
            {
                throw new DirectoryNotFoundException(
                    $"Directory '{InWorkOptions.Target}' not found and impossible to create.");
            }

            WarnForCompressedTargetDir(InWorkOptions.Target);
            MainTestFilesNotAlreadyExist(InWorkOptions.Source, InWorkOptions.Target, partFileMaxLenght, !InWorkOptions.CanOverwrite);

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

                        AppFile fileOutPath = new AppFile(InWorkOptions.Target, FileUtils.GetFileName(InWorkOptions.Source.Name, fs.Length, fileCreatedIndex));

                        DateTime localStart = DateTime.Now;


                        long localBytesRead = WritePartFile(partFileMaxLenght, pbar, fs, fileOutPath, localStart, 3);

                        fileOutPath.File.Attributes = FileAttributes.Hidden | FileAttributes.Archive |
                                                      FileAttributes.Temporary | FileAttributes.NotContentIndexed;
                        listFiles.Add(fileOutPath.File);
                        _log.Debug("> OK");

                        if (_totalBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                        {
                            WaitForFreeSpace(listFiles);
                        }

                        if (isFirstFileCreated)
                        {
                            string tsftFileContent = GetTransferExchangeFileContent(InWorkOptions.Source.Name, InWorkOptions.Source.Length,
                                partFileMaxLenght, sha1);
                            if (tsftFileContent != null)
                            {
                                InWorkOptions.TsftFilePath = Path.Combine(InWorkOptions.Target, InWorkOptions.Source.Name + ".tsft");
                                File.WriteAllText(InWorkOptions.TsftFilePath, tsftFileContent, Encoding.UTF8);
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

            Console.Title = "";
        }

        private long WritePartFile(long partFileMaxLenght, ProgressBar pbar, FileStream fs, AppFile fileOutPath, DateTime localStart, int nbTentative)
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

        private long WritePartFile(long chunk, ProgressBar pbar, FileStream fs, AppFile fileOutPath, DateTime localStart)
        {
            long localBytesRead = 0;

            try
            {

                using (FileStream fo = new FileStream(fileOutPath.TempFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None, InWorkOptions.BufferSize, false))
                {
                    fo.SetLength(Math.Min(chunk, InWorkOptions.Source.Length - _totalBytesRead));

                    string msg = "Creating part file " + fileOutPath.File.Name;
                    Console.Title = $"TSFT - In - {msg}";
                    _log.Debug(msg);

                    Array.Clear(_buffer, 0, _buffer.Length);
                    int bytesRead;

                    while ((bytesRead = fs.Read(_buffer, 0, _buffer.Length)) > 0)
                    {
                        _totalBytesRead += bytesRead;
                        localBytesRead += bytesRead;

                        fo.Write(_buffer, 0, bytesRead);

                        if (localBytesRead + InWorkOptions.BufferSize > chunk ||
                            localBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                        {
                            break;
                        }
                    }

                }

                pbar.Report((double)_totalBytesRead / fs.Length, AppUtils.GetTransferSpeed(localBytesRead, localStart));

                fileOutPath.MoveToNormal();
            }
            catch (Exception ex)
            {
                if (fileOutPath.TempFile.Exists)
                {
                    fileOutPath.TempFile.Delete();
                    
                }
                throw  ex;
            }

            return localBytesRead;
        }

        protected override void TestFilesNotAlreadyExist(FileInfo source, string target, long chunkSize, bool exceptionIfExists = false)
        {
            long nbFiles = (source.Length / chunkSize) + (source.Length % chunkSize == 0 ? 0 : 1);
            for (long i = 0; i < nbFiles; i++)
            {
                String tmpFile = Path.Combine(target, "~" + FileUtils.GetFileName(InWorkOptions.Source.Name, source.Length, i));
                if (File.Exists(tmpFile))
                {
                    _log.Debug("File {0} already exists.", tmpFile);
                    File.Delete(tmpFile);
                }

                String realPartFile = Path.Combine(target, FileUtils.GetFileName(InWorkOptions.Source.Name, source.Length, i));
                if (File.Exists(realPartFile))
                {
                    if (exceptionIfExists)
                    {
                        throw new IOException($"File {realPartFile} already exists.");
                    }

                    _log.Warn("File {0} already exists.", realPartFile);
                    File.Delete(realPartFile);

                }
            }
        }

        protected override void IncludeMoreThingsInTsftFile(TsftFile tsftFile)
        {
            // Nothing to add here;
        }

        private void WarnForCompressedTargetDir(string target)
        {
            DirectoryInfo targetDir = new DirectoryInfo(target);
            if (targetDir.Attributes.HasFlag(FileAttributes.Compressed))
            {
                LogUtils.I(_log, $"Target '{target}' is compressed, this can lead to degraded performance");
            }

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                if (drive.RootDirectory.FullName.Equals(targetDir.Root.FullName.ToUpper()))
                {
                    if (drive.DriveType == DriveType.Network)
                    {
                        LogUtils.I(_log, $"Target '{target}' is on a network drive, this can lead to degraded performance");
                    }
                    else if (drive.DriveType == DriveType.Removable)
                    {
                        LogUtils.I(_log, $"Target '{target}' is on a removable drive, this can lead to degraded performance");
                    }

                }

            }
        }



        private void WaitForFreeSpace(HashSet<FileInfo> listFiles)
        {
            bool mustWriteLogStatus = true;
            long filesSize = listFiles.Where(r =>
                {
                    r.Refresh();
                    return r.Exists;
                })
                .Sum(f => f.Length);

            TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
            while (filesSize + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
            {
                HashSet<FileInfo> setFilesExist = new HashSet<FileInfo>(listFiles.Where(r =>
                {
                    r.Refresh();
                    return r.Exists;
                }).ToList());

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

        /*
        private void GetTransferExchangeFileContent(string originalFileName, long originalFileSize, string sha1)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine(originalFileName);
            s.AppendLine(originalFileSize.ToString());
            s.AppendLine(sha1);

            String transfertFile = Path.Combine(InWorkOptions.Target, InWorkOptions.Source.Name + ".tsft");
            File.WriteAllText(transfertFile, s.ToString(), Encoding.UTF8);
        }
        */



        protected override long CalculatePartFileMaxLenght()
        {
            long partFileMaxLenght = InWorkOptions.PartFileSize;
            if (partFileMaxLenght == -1)
            {
                partFileMaxLenght = Math.Min(InWorkOptions.MaxSizeUsedOnShared / 10, 50 * 1024 * 1024);
            }

            partFileMaxLenght = new[]
            {
                InWorkOptions.MaxSizeUsedOnShared, partFileMaxLenght, InWorkOptions.Source.Length
            }.Min();

            LogUtils.I(_log, $"Part file size: {AFileUtils.HumanReadableSize(partFileMaxLenght)}");
            return partFileMaxLenght;
        }


    }
}
