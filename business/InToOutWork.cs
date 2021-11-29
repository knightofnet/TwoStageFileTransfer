using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            long chunk = 10 * 1024 * 1024;

            int i = 0;

            HashSet<FileInfo> listFiles = new HashSet<FileInfo>();

            using (FileStream fs = new FileStream(Source.FullName, FileMode.Open))
            {
                while (totalBytesRead < fs.Length)
                {
                    long localBytesRead = 0;
                    String fileOut = FileUtils.GetFileName(Source.Name, fs.Length, i++);
                    FileInfo fileOutPath = new FileInfo(Path.Combine(Target, "~" + fileOut));

                    String msg = "";
                    using (FileStream fo = new FileStream(fileOutPath.FullName, FileMode.Create))
                    {
                        msg = "Ecriture du fichier " + fileOut;
                        Console.Title = String.Format("TSFT - In - {0}", msg);
                        Console.Write(msg);

                        byte[] buffer = new byte[BufferSize];
                        int bytesRead;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            localBytesRead += bytesRead;

                            fo.Write(buffer, 0, bytesRead);

                            if (localBytesRead + BufferSize > chunk || localBytesRead + BufferSize > MaxTransfertLength)
                            {
                                break;
                            }
                        }


                    }
                    fileOutPath.MoveTo(Path.Combine(Target, fileOut));
                    listFiles.Add(fileOutPath);

                    msg = " [OK] ";
                    Console.WriteLine(msg);


                    if (totalBytesRead + BufferSize > MaxTransfertLength)
                    {
                        long filesSize;
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

                            Console.Title = String.Format("TSFT - In - {0}", "En attente de libération d'espace disque...");
                            Thread.Sleep(500);
                        } while (filesSize + BufferSize > MaxTransfertLength);
                    }
                } // while for Write
            }
        }
    }
}
