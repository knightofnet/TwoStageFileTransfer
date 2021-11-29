using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    class InToOutWork : AbstractWork
    {
        public long MaxTransfertLength { get; internal set; }

        public void DoTransfert()
        {

            long totalBytesRead = 0;

            long chunk = Math.Min(MaxTransfertLength / 5 , 50 * 1024 * 1024);

            int i = 0;

            HashSet<FileInfo> listFiles = new HashSet<FileInfo>();

            Console.Write("Calculate SHA1... ");
            string sha1 = FileUtils.GetSha1Hash(Source);
            Console.WriteLine("Done.");

            Console.Write("Creating part files... ");

            using (ProgressBar pbar = new ProgressBar())
            {
                using (FileStream fs = new FileStream(Source.FullName, FileMode.Open, FileAccess.Read,FileShare.Read, BufferSize, FileOptions.SequentialScan ))
                {
                   
                    while (totalBytesRead < fs.Length)
                    {
                        long localBytesRead = 0;
                        String fileOut = FileUtils.GetFileName(Source.Name, fs.Length, i);
                        FileInfo fileOutPath = new FileInfo(Path.Combine(Target, "~" + fileOut));

                        String msg = "";
                        using (FileStream fo = new FileStream(fileOutPath.FullName, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, false))
                        {
                            msg = "Out part file " + fileOut;
                            Console.Title = String.Format("TSFT - In - {0}", msg);
                            _log.Debug(msg);

                            byte[] buffer = new byte[BufferSize];
                            int bytesRead;

                            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytesRead += bytesRead;
                                localBytesRead += bytesRead;
                                pbar.Report((double)totalBytesRead / fs.Length);

                                fo.Write(buffer, 0, bytesRead);

                                if (localBytesRead + BufferSize > chunk || localBytesRead + BufferSize > MaxTransfertLength)
                                {
                                    break;
                                }
                            }


                        }
                        fileOutPath.MoveTo(Path.Combine(Target, fileOut));
                        listFiles.Add(fileOutPath);
                        _log.Debug("> OK");

                        if (totalBytesRead + BufferSize > MaxTransfertLength)
                        {
                            WaitForFreeSpace(listFiles);
                        }

                        if (i == 0)
                        {
                            WriteTransferExchangeFile(Source.Name, Source.Length, sha1);
                        }
                        i++;
                    } // while for Write
                }
            }
            Console.WriteLine("Done.");

        }

  

        private void WaitForFreeSpace(HashSet<FileInfo> listFiles)
        {
            bool mustWriteLogStatus = true;
            long filesSize;

            TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
            do
            {
                filesSize = 0;
                foreach (FileInfo f in listFiles.Where(r =>
                {
                    r.Refresh();
                    return r.Exists;
                }))
                {
                    filesSize += f.Length;
                }

                Console.Title = String.Format("TSFT - In - {0}", "Waiting for OUT mode to work and freeing disk space...");
                if (mustWriteLogStatus)
                {
                    _log.Info("Waiting for OUT mode to work and freeing disk space : {0} + {1} > {2}", filesSize, BufferSize, MaxTransfertLength);
                    mustWriteLogStatus = false;
                }
                Thread.Sleep(500);

                if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
                {
                    throw new Exception("Waits too long time for disk space");
                }

            } while (filesSize + BufferSize > MaxTransfertLength);
        }

        private void WriteTransferExchangeFile(string originalFileName, long originalFileSize, string sha1)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine(originalFileName);
            s.AppendLine(originalFileSize.ToString());
            s.AppendLine(sha1);


            String transfertFile = Path.Combine(Target, Source.Name + ".tsft");
            File.WriteAllText(transfertFile, s.ToString(), Encoding.UTF8);
        }
    }
}
