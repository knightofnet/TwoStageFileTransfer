using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    class OutToFileWork : AbstractWork
    {


        internal void DoTransfert()
        {

            if (Source == null)
            {
                Console.WriteLine("Error : no first file provided");
                return;
            }
            else if ( !Source.Exists && !AryxDevLibrary.utils.FileUtils.IsADirectory(Source.FullName))
            {
                // ERROR
                Console.WriteLine("Error : '{0}' doesnt exist", Source.FullName);
                return;
            }

            bool isFirstForWaitMsg = true;
            while (AryxDevLibrary.utils.FileUtils.IsADirectory(Source.FullName))
            {
                DirectoryInfo d = new DirectoryInfo(Source.FullName);
                FileInfo[] ret = d.GetFiles("*.part0", SearchOption.TopDirectoryOnly);

                bool isFound = false;
                foreach (FileInfo candidatFirstFile in ret)
                {
                    if (candidatFirstFile.Name.StartsWith("~"))
                    {
                        continue;
                    }

                    if (AppCst.FirstFilePatternRegex.IsMatch(candidatFirstFile.Name))
                    {
                        Source = candidatFirstFile;
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    if (isFirstForWaitMsg)
                    {
                        Console.WriteLine("Wait for transfert files to be generated");
                        isFirstForWaitMsg = false;
                    }
                    Thread.Sleep(2000);
                }
               
            }

            Match m = AppCst.FilePatternRegex.Match(Source.Name);
            if (!m.Success)
            {
                // ERRROR
                Console.WriteLine("Error : '{0}' is not a first valid file.", Source.FullName);
                return;
            }


            long totalBytesRead = 0;

            long totalBytesToRead = long.Parse(m.Groups["size"].Value);
            String finalFileName = m.Groups["name"].Value;

            FileInfo targetFile = new FileInfo(Path.Combine(Target, "~" + finalFileName));
            FileInfo rTargetFile = new FileInfo(Path.Combine(Target, finalFileName));
            if (rTargetFile.Exists)
            {
                _log.Warn("{0} already exists : delete");
                rTargetFile.Delete();
            }


            int i = int.Parse(m.Groups["part"].Value) + 1;

            

            Console.Write("Recomposing file... ");

            using (ProgressBar pbar = new ProgressBar())
            {
                using (FileStream fo = new FileStream(targetFile.FullName, FileMode.Create))
                {
                    FileInfo currentFileToRead = Source;

                    while (totalBytesRead < totalBytesToRead)
                    {

                        TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
                        while (!currentFileToRead.Exists || AryxDevLibrary.utils.FileUtils.IsFileLocked(currentFileToRead))
                        {
                            fo.Flush();
                            Console.Title = String.Format("TSFT - Out - Waiting for {0}", currentFileToRead.FullName);
                            Thread.Sleep(500);
                            currentFileToRead.Refresh();

                            if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
                            {
                                throw new Exception(String.Format("Waits too long time for {0}", currentFileToRead.FullName));
                            }
                        }

                        String msg = "Reading file " + currentFileToRead.Name;
                        Console.Title = String.Format("TSFT - Out - {0}", msg);
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
            }

            targetFile.MoveTo(rTargetFile.FullName);
            Console.WriteLine("Done.");

            return;
        }
    }
}
