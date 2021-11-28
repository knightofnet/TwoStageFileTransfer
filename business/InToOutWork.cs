using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    class InToOutWork
    {
        public FileInfo FileInput { get; internal set; }
        public string Target { get; set; }



        public void DoTransfert(long maxTransfert)
        {

            long totalBytesRead = 0;

            long chunk = 2 * 1024 * 1024;

            int i = 0;

            List<FileInfo> listFiles = new List<FileInfo>();

            using (FileStream fs = new FileStream(FileInput.FullName, FileMode.Open))
            {
                while (totalBytesRead < fs.Length)
                {
                    long localBytesRead = 0;

                    String fileOut = FileUtils.GetFileName(FileInput.Name, fs.Length, i++);

                    FileInfo fileOutPath = new FileInfo(Path.Combine(Target, "~" + fileOut));

                    using (FileStream fo = new FileStream(fileOutPath.FullName, FileMode.Create))
                    {
                        byte[] buffer = new byte[AppCst.BufferSize];
                        int bytesRead;

                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            localBytesRead += bytesRead;

                            fo.Write(buffer, 0, bytesRead);

                            if (localBytesRead + AppCst.BufferSize > chunk || localBytesRead + AppCst.BufferSize > maxTransfert)
                            {
                                break;
                            }
                        }


                    }
                    fileOutPath.MoveTo(Path.Combine(Target, fileOut));
                    listFiles.Add(fileOutPath);


                    if (totalBytesRead + AppCst.BufferSize > maxTransfert)
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

                            Console.WriteLine(".");
                            Thread.Sleep(2000);
                        } while (filesSize + AppCst.BufferSize > maxTransfert);
                    }
                } // while for Write
            }
        }
    }
}
