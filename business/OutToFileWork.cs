using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    class OutToFileWork : AbstractWork
    {
        private FileInfo _firstFile = null;

        internal void DoTransfert()
        {

            if (Source == null)
            {
                Console.WriteLine("Error : no first file provided");
                return;
            }
            else if (!Source.Exists && !AryxDevLibrary.utils.FileUtils.IsADirectory(Source.FullName))
            {
                // ERROR
                Console.WriteLine("Error : '{0}' doesnt exist", Source.FullName);
                return;
            }



            long totalBytesRead = 0;
            long totalBytesToRead = 0;
            String finalFileName = null;
            int i = 1;

            if (Source.Name.ToUpper().EndsWith(".TSFT") && !AryxDevLibrary.utils.FileUtils.IsADirectory(Source.FullName))
            {
                String[] configFile = File.ReadAllLines(Source.FullName, Encoding.UTF8);

                totalBytesToRead = long.Parse(configFile[1].Trim());
                finalFileName = configFile[0].Trim();

                _firstFile = new FileInfo(Path.Combine(Source.DirectoryName, FileUtils.GetFileName(finalFileName, totalBytesToRead, 0)));

            }
            else
            {
                _firstFile = Source;
                if (AryxDevLibrary.utils.FileUtils.IsADirectory(Source.FullName))
                {
                    _firstFile = FindFirstFileSourceDir(Source);
                }

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

            FileInfo targetFile = new FileInfo(Path.Combine(Target, "~" + finalFileName));
            FileInfo rTargetFile = new FileInfo(Path.Combine(Target, finalFileName));
            if (rTargetFile.Exists)
            {
                _log.Warn("{0} already exists : delete");
                rTargetFile.Delete();
                rTargetFile.Refresh();
            }

            Console.Write("Recomposing file... ");

            using (ProgressBar pbar = new ProgressBar())
            using (FileStream fo = new FileStream(targetFile.FullName, FileMode.Create))
            {
                fo.SetLength(totalBytesToRead);

                FileInfo currentFileToRead = _firstFile;

                while (totalBytesRead < totalBytesToRead)
                {

                    TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
                    while (!currentFileToRead.Exists || AryxDevLibrary.utils.FileUtils.IsFileLocked(currentFileToRead))
                    {
                        fo.Flush();
                        Console.Title = string.Format("TSFT - Out - Waiting for {0}", currentFileToRead.FullName);
                        Thread.Sleep(500);
                        currentFileToRead.Refresh();

                        if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
                        {
                            throw new Exception(string.Format("Waits too long time for {0}", currentFileToRead.FullName));
                        }
                    }

                    string msg = "Reading file " + currentFileToRead.Name;
                    Console.Title = string.Format("TSFT - Out - {0}", msg);
                    _log.Debug(msg);

                    using (FileStream fr = new FileStream(currentFileToRead.FullName, FileMode.Open))
                    {

                        byte[] buffer = new byte[BufferSize];
                        int bytesRead;

                        while ((bytesRead = fr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            fo.Write(buffer, 0, bytesRead);
                        }
                        pbar.Report((double)totalBytesRead / totalBytesToRead);

                    }

                    _log.Debug("> OK");
                    currentFileToRead.Delete();
                    _log.Debug("> File part deleted");


                    currentFileToRead = new FileInfo(Path.Combine(Source.Directory.FullName, FileUtils.GetFileName(finalFileName, totalBytesToRead, i++)));


                }


            }

            while (AryxDevLibrary.utils.FileUtils.IsFileLocked(targetFile))
            {
                _log.Debug("> Quasi-done : {0} locked", targetFile.FullName);
                Thread.Sleep(500);
            }

            targetFile.MoveTo(rTargetFile.FullName);
            Console.WriteLine("Done.");

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
