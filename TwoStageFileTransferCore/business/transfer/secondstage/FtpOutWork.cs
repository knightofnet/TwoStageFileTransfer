using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using AryxDevLibrary.utils;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.utils;
using FileUtils = TwoStageFileTransferCore.utils.FileUtils;

namespace TwoStageFileTransferCore.business.transfer.secondstage
{
    public class FtpOutWork : AbstractOutWork
    {
        protected long TotalBytesRead;
        protected byte[] Buffer;

        protected IConnexion Connexion;

        private AppFileFtp _firstFile;

        public FtpOutWork(IConnexion connexion, OutWorkOptions outWorkOptions) : base(outWorkOptions)
        {
            Connexion = connexion;
        }


        public override void DoTransfert(IProgressTransfer transferReporter)
        {

            long totalBytesRead = 0;
            long totalBytesToRead;
            int i = 1;
            string finalFileName;

            string sha1FinalFile;
            if (Options.Tsft != null)
            {
                totalBytesToRead = Options.Tsft.FileLenght;
                finalFileName = Options.Tsft.Source.OriginalFilename;
                sha1FinalFile = Options.Tsft.Sha1Hash;

                _firstFile = new AppFileFtp(Options.Tsft.TempDir.Path, TwoStageFileTransferCore.utils.FileUtils.GetFileName(finalFileName, totalBytesToRead, 0));
            }
            else
            {
                throw new Exception("A TERMINER");

            }



            FileInfo targetFile = new FileInfo(Path.Combine(Options.Target, "~" + finalFileName));
            FileInfo rTargetFile = new FileInfo(Path.Combine(Options.Target, finalFileName));
            TestFileAlreadyExists(rTargetFile);


            Console.Write("Recomposing file... ");
            DateTime mainStart = DateTime.Now;

            transferReporter.Init();
            using (FileStream fo = new FileStream(targetFile.FullName, FileMode.Create))
            {
                fo.SetLength(totalBytesToRead);

                AppFileFtp currentFileToRead = _firstFile;


                // TODO FtpUtils.LogListDir(currentFileToRead.DirectoryParent, Connexion);
                Buffer = new byte[Options.BufferSize];

                while (totalBytesRead < totalBytesToRead)
                {

                    TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
                    while (!currentFileToRead.Exists(Connexion))
                    {
                        fo.Flush();
                        Console.Title = $"TSFT - Out - Waiting for {currentFileToRead.File.AbsolutePath}";
                        transferReporter.Report((double)totalBytesRead / totalBytesToRead, $"waiting for part {i - 1}");
                        Thread.Sleep(300);

                        if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
                        {
                            throw new Exception($"Waits too long time for {currentFileToRead.File.AbsolutePath}");
                        }
                    }

                    totalBytesRead = ReadPartFile(currentFileToRead, totalBytesRead, fo, transferReporter, totalBytesToRead);

                    currentFileToRead = new AppFileFtp(Options.Tsft.TempDir.Path, TwoStageFileTransferCore.utils.FileUtils.GetFileName(finalFileName, totalBytesToRead, i++));

                }

            }

            while (AryxDevLibrary.utils.FileUtils.IsFileLocked(targetFile))
            {
                _log.Debug("> Almost done : {0} locked", targetFile.FullName);
                Thread.Sleep(500);
            }

            if (!Options.KeepPartFiles && Options.AppArgs.TsftFile != null)
            {
                Options.Source.Delete();
                Uri remoteTsftFile = FtpUtils.FtpPathCombine(Options.Tsft.TempDir.Path, Options.Source.Name).ToUri();
                if (Connexion.IsFileExists(remoteTsftFile))
                {
                    Connexion.DeleteFile(remoteTsftFile);
                }
            }
            targetFile.MoveTo(rTargetFile.FullName);

            transferReporter.Dispose();
            Console.WriteLine("Done.");
            TimeSpan duration = DateTime.Now - mainStart;
            _log.Info("> Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));

            TwoStageFileTransferCore.utils.FileUtils.CalculculateSourceSha1(targetFile, sha1FinalFile);


        }

        protected virtual long ReadPartFile(AppFileFtp currentFileToRead, long totalBytesRead, FileStream fo, IProgressTransfer transfertReporter, long totalBytesToRead)
        {
            string msg = "Reading file " + currentFileToRead.File.Segments.Last();
            Console.Title = $"TSFT - Out - {msg}";
            _log.Debug(msg);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentFileToRead.File);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = Connexion.Credentials;
            request.KeepAlive = true;
            request.UsePassive = true;
            request.UseBinary = true;

            DateTime localStart = DateTime.Now;
            long localBytesRead = 0;

            using (Stream fr = request.GetResponse().GetResponseStream())
            {
                Array.Clear(Buffer, 0, Buffer.Length);
                int bytesRead;

                while (fr != null && (bytesRead = fr.Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    totalBytesRead += bytesRead;
                    localBytesRead += bytesRead;
                    fo.Write(Buffer, 0, bytesRead);
                    transfertReporter.Report((double)totalBytesRead / totalBytesToRead,
                        CommonAppUtils.GetTransferSpeed(localBytesRead, localStart));
                }
            }

            _log.Debug("> OK");
            if (!Options.KeepPartFiles)
            {
                currentFileToRead.Delete(Connexion);
                _log.Debug("> File part deleted");
            }

            return totalBytesRead;
        }


        private static FileInfo FindFirstFileSourceDir(FileInfo source)
        {
            bool isFirstForWaitMsg = true;

            while (AryxDevLibrary.utils.FileUtils.IsADirectory(source.FullName))
            {
                DirectoryInfo d = new DirectoryInfo(source.FullName);
                FileInfo[] ret = d.GetFiles("*.part0", SearchOption.TopDirectoryOnly);

                foreach (FileInfo candidatFirstFile in ret)
                {
                    if (candidatFirstFile.Name.StartsWith("~"))
                    {
                        continue;
                    }

                    if (AppCst.FirstFilePatternRegex.IsMatch(candidatFirstFile.Name))
                    {
                        return candidatFirstFile;
                    }
                }

                if (isFirstForWaitMsg)
                {
                    Console.WriteLine("Wait for transfert files to be generated");
                    isFirstForWaitMsg = false;
                }

            }

            return null;
        }
    }
}
