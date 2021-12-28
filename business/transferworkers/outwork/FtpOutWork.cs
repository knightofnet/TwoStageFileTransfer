using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using FileUtils = TwoStageFileTransfer.utils.FileUtils;

namespace TwoStageFileTransfer.business.transferworkers.outwork
{
    class FtpOutWork : AbstractOutWork
    {
        private long _totalBytesRead = 0;
        private byte[] _buffer;

        private  NetworkCredential _ftpCredentials;

        private AppFileFtp _firstFile = null;

        public FtpOutWork(NetworkCredential networkCredential, OutWorkOptions outWorkOptions) : base(outWorkOptions)
        {
            _ftpCredentials = networkCredential;
        }


        public override void DoTransfert()
        {

            long totalBytesRead = 0;
            long totalBytesToRead = 0;
            String finalFileName = null;
            int i = 1;

            string sha1FinalFile = null;

            if (Options.Tsft != null)
            {
                totalBytesToRead = Options.Tsft.FileLenght;
                finalFileName = Options.Tsft.Source.OriginalFilename;
                sha1FinalFile = Options.Tsft.Sha1Hash;

                if (String.IsNullOrWhiteSpace(_ftpCredentials.UserName))
                {
                    _ftpCredentials = new NetworkCredential(Options.Tsft.TempDir.FtpUsername,
                        Options.Tsft.TempDir.FtpPassword);
                }

                Uri serverHost = FtpUtils.GetRootUri(FtpUtils.NewFtpUri(Options.Tsft.TempDir.Path));
                if (!FtpUtils.IsOkToConnect(serverHost, _ftpCredentials))
                {
                    throw new Exception($"Unable to established a connexion to ${serverHost.AbsoluteUri}");
                }

                _firstFile = new AppFileFtp(Options.Tsft.TempDir.Path, FileUtils.GetFileName(finalFileName, totalBytesToRead, 0), _ftpCredentials );
            }
            else
            {
                throw new Exception("A TERMINER");

            }

            

            FileInfo targetFile = new FileInfo(Path.Combine(Options.Target, "~" + finalFileName));
            FileInfo rTargetFile = new FileInfo(Path.Combine(Options.Target, finalFileName));
            if (rTargetFile.Exists)
            {
                _log.Warn("{0} already exists : delete");
                rTargetFile.Delete();
                rTargetFile.Refresh();
            }

            Console.Write("Recomposing file... ");
            DateTime mainStart = DateTime.Now;
            using (ProgressBar pbar = new ProgressBar())
            using (FileStream fo = new FileStream(targetFile.FullName, FileMode.Create))
            {
                fo.SetLength(totalBytesToRead);

                AppFileFtp currentFileToRead = _firstFile;

                FtpUtils.LogListDir(currentFileToRead.DirectoryParent, _ftpCredentials);

                while (totalBytesRead < totalBytesToRead)
                {

                    TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
                    while (!currentFileToRead.Exists() )
                    {
                        fo.Flush();
                        Console.Title = string.Format("TSFT - Out - Waiting for {0}", currentFileToRead.File.AbsolutePath);
                        Thread.Sleep(300);
                       

                        if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
                        {
                            throw new Exception(string.Format("Waits too long time for {0}", currentFileToRead.File.AbsolutePath));
                        }
                    }

                    string msg = "Reading file " + currentFileToRead.File.Segments.Last();
                    Console.Title = string.Format("TSFT - Out - {0}", msg);
                    _log.Debug(msg);

                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(currentFileToRead.File);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.Credentials = _ftpCredentials;
                    request.KeepAlive = true;
                    request.UsePassive = true;
                    request.UseBinary = true;

                    byte[] buffer = new byte[Options.BufferSize];
                    using (Stream fr = request.GetResponse().GetResponseStream())
                    {

                        Array.Clear(buffer, 0, buffer.Length);
                        int bytesRead;

                        while ((bytesRead = fr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            fo.Write(buffer, 0, bytesRead);
                        }
                        pbar.Report((double)totalBytesRead / totalBytesToRead, "");

                    }

                    _log.Debug("> OK");
                    currentFileToRead.Delete();
                    _log.Debug("> File part deleted");

                    currentFileToRead = new AppFileFtp(Options.Tsft.TempDir.Path, FileUtils.GetFileName(finalFileName, totalBytesToRead, i++), _ftpCredentials);

                }

            }

            while (AryxDevLibrary.utils.FileUtils.IsFileLocked(targetFile))
            {
                _log.Debug("> Almost done : {0} locked", targetFile.FullName);
                Thread.Sleep(500);
            }

            targetFile.MoveTo(rTargetFile.FullName);
            Console.WriteLine("Done.");
            TimeSpan duration = DateTime.Now - mainStart;
            _log.Info("> Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));

            FileUtils.CalculculateSourceSha1(targetFile, sha1FinalFile);

            return;
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
