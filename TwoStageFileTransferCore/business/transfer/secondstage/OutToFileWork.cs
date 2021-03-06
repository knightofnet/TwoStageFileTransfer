using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferCore.business.transfer.secondstage
{
    public class OutToFileWork : AbstractOutWork
    {
        private FileInfo _firstFile ;

        public OutToFileWork(OutWorkOptions outToFileWork) : base(outToFileWork)
        {

        }

        public override void DoTransfert(IProgressTransfer transferReporter)
        {
            

            long totalBytesRead = 0;
            long totalBytesToRead = 0;
            String finalFileName = null;

            string sourceDir = null;

            int i = 1;

            string sha1FinalFile = null;

            if (Options.Tsft != null)
            {
                totalBytesToRead = Options.Tsft.FileLenght;
                finalFileName = Options.Tsft.Source.OriginalFilename;
                sha1FinalFile = Options.Tsft.Sha1Hash;

                sourceDir = Options.Source.DirectoryName;

                _firstFile = new FileInfo(Path.Combine(Options.Source.DirectoryName, FileUtils.GetFileName(finalFileName, totalBytesToRead, 0)));

            }
            else
            {
                _firstFile = Options.Source;
                if (AryxDevLibrary.utils.FileUtils.IsADirectory(Options.Source.FullName))
                {
                    _firstFile = FindFirstFileSourceDir(Options.Source);
                }
                sourceDir = _firstFile.DirectoryName;

                Match m = AppCst.FilePatternRegex.Match(_firstFile.Name);
                if (!m.Success)
                {
                    // ERRROR
                    Console.WriteLine("Error : '{0}' is not a first valid file.", _firstFile.FullName);
                    return;
                }

                totalBytesToRead = long.Parse(m.Groups["size"].Value);
                finalFileName = m.Groups["name"].Value;
                i = int.Parse(m.Groups["part"].Value) + 1;

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

                FileInfo currentFileToRead = _firstFile;

                byte[] buffer = new byte[Options.BufferSize];

                while (totalBytesRead < totalBytesToRead)
                {

                    TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
                    while (!currentFileToRead.Exists || AryxDevLibrary.utils.FileUtils.IsFileLocked(currentFileToRead))
                    {
                        fo.Flush();
                        transferReporter.OnProgress($"TSFT - Out - Waiting for {currentFileToRead.FullName}");
                        transferReporter.OnProgress($"waiting for part {i}", (double)totalBytesRead / totalBytesToRead,
                            BckgerReportType.ProgressPbarText);
                        Thread.Sleep(300);
                        currentFileToRead.Refresh();

                        if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(Options.NbMinWaitForFreeSpace))
                        {
                            throw new Exception($"Waits too long time for {currentFileToRead.FullName}");
                        }
                    }

                    string msg = "Reading file " + currentFileToRead.Name;
                    transferReporter.OnProgress($"TSFT - Out - {msg}");
                    _log.Debug(msg);

                    DateTime localStart = DateTime.Now;
                    long localBytesRead = 0;

                    using (FileStream fr = new FileStream(currentFileToRead.FullName, FileMode.Open))
                    {

                        Array.Clear(buffer, 0, buffer.Length);
                        int bytesRead;

                        while ((bytesRead = fr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            localBytesRead += bytesRead;
                            fo.Write(buffer, 0, bytesRead);
                            transferReporter.OnProgress(CommonAppUtils.GetTransferSpeed(localBytesRead, localStart), (double)totalBytesRead / totalBytesToRead,
                                BckgerReportType.ProgressPbarText);
                        }
                        

                    }

                    _log.Debug("> OK");
                    if (!Options.KeepPartFiles)
                    {
                        currentFileToRead.Delete();
                        _log.Debug("> File part deleted");
                    }


                    currentFileToRead = new FileInfo(Path.Combine(sourceDir, FileUtils.GetFileName(finalFileName, totalBytesToRead, i++)));
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
            }
            targetFile.MoveTo(rTargetFile.FullName);

            transferReporter.Dispose();
            Console.WriteLine("Done.");
            TimeSpan duration = DateTime.Now - mainStart;
            _log.Info("> Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));

            FileUtils.CalculculateSourceSha1(targetFile, sha1FinalFile);

           
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
