using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    class InToOutWork : AbstractWork
    {

        private readonly long _maxTransferFile;
        private readonly long _chunkSize;
        private readonly bool _isDoCompressBefore;

        public long MaxTransfertLength => _maxTransferFile;

        public InToOutWork(long maxTransfertLength, long chunkSize, bool isDoCompressBefore = false)
        {
            _maxTransferFile = maxTransfertLength;
            _chunkSize = chunkSize;
            _isDoCompressBefore = isDoCompressBefore;
        }
        public void DoTransfert()
        {

            long totalBytesRead = 0;

            long chunk = _chunkSize;
            if (chunk == -1)
            {
                chunk = Math.Min((long) _maxTransferFile / 10, 50 * 1024 * 1024);
            }
            
            //chunk = 16 * 1024;

            int i = 0;

            HashSet<FileInfo> listFiles = new HashSet<FileInfo>();

            Console.Write("Calculate SHA1... ");
            string sha1 = FileUtils.GetSha1Hash(Source);
            Console.WriteLine("Done.");

            if (_isDoCompressBefore)
            {
                FileInfo newSource = new FileInfo(Path.GetTempFileName());
                if (newSource.Exists) newSource.Delete();
                using (var archive = ZipFile.Open(newSource.FullName, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(Source.FullName, Source.Name, CompressionLevel.Optimal);
                }

                Source = newSource;

            }

            Console.Write("Creating part files... ");
            using (ProgressBar pbar = new ProgressBar())
            {
                using (FileStream fs = new FileStream(Source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan))
                {

                    while (totalBytesRead < fs.Length)
                    {
                        long localBytesRead = 0;
                        AppFile fileOutPath = new AppFile(Target, FileUtils.GetFileName(Source.Name, fs.Length, i))
                        {
                            IsNormalFile = false
                        };

                        byte[] buffer = new byte[BufferSize];
                        using (FileStream fo = new FileStream(fileOutPath.FullName, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, false))
                        {
                            fo.SetLength(Math.Min(chunk, Source.Length - totalBytesRead )); 

                            string msg = "Out part file " + fileOutPath.GetNormalFileInfo().Name;
                            Console.Title = string.Format("TSFT - In - {0}", msg);
                            _log.Debug(msg);

                            Array.Clear(buffer, 0, buffer.Length);
                            int bytesRead;

                            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytesRead += bytesRead;
                                localBytesRead += bytesRead;

                                fo.Write(buffer, 0, bytesRead);

                                if (localBytesRead + BufferSize > chunk || localBytesRead + BufferSize > _maxTransferFile)
                                {
                                    break;
                                }
                            }

                        }
                        pbar.Report((double)totalBytesRead / fs.Length);

                        fileOutPath.IsNormalFile = true;
                        fileOutPath.MoveToNormal();
                        fileOutPath.File.Attributes = FileAttributes.Hidden | FileAttributes.Archive |
                                                      FileAttributes.Temporary | FileAttributes.NotContentIndexed;
                        listFiles.Add(fileOutPath.File);
                        _log.Debug("> OK");

                        if (totalBytesRead + BufferSize > _maxTransferFile)
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
            long filesSize = listFiles.Where(r =>
                {
                    r.Refresh();
                    return r.Exists;
                })
                .Sum(f => f.Length);

            TimeSpan nowBeforeWait = DateTime.Now.TimeOfDay;
            while (filesSize + BufferSize > _maxTransferFile)
            {
                filesSize = listFiles.Where(r =>
                    {
                        r.Refresh();
                        return r.Exists;
                    })
                    .Sum(f => f.Length);

                Console.Title = string.Format("TSFT - In - {0}", "Waiting for OUT mode to work and freeing disk space...");
                if (mustWriteLogStatus)
                {
                    _log.Info("Waiting for OUT mode to work and freeing disk space : {0} + {1} > {2}", filesSize, BufferSize, _maxTransferFile);
                    mustWriteLogStatus = false;
                }
                Thread.Sleep(500);

                if (DateTime.Now.TimeOfDay > nowBeforeWait + TimeSpan.FromMinutes(5))
                {
                    throw new Exception("Waits too long time for disk space");
                }

            }
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
