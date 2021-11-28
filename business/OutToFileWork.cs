using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    class OutToFileWork
    {
        public FileInfo FirstFile { get; internal set; }
        public string Target { get; internal set; }

        internal void DoTransfert()
        {

            if (FirstFile == null)
            {
                Console.WriteLine("Erreur : aucun premier fichier saisi");
                return;
            }
            else if ( !FirstFile.Exists && !AryxDevLibrary.utils.FileUtils.IsADirectory(FirstFile.FullName))
            {
                // ERROR
                Console.WriteLine("Erreur : '{0}' n'existe pas", FirstFile.FullName);
                return;
            }

            while (AryxDevLibrary.utils.FileUtils.IsADirectory(FirstFile.FullName))
            {
                DirectoryInfo d = new DirectoryInfo(FirstFile.FullName);
                FileInfo[] ret = d.GetFiles("*.part0", SearchOption.TopDirectoryOnly);

                bool isFound = false;
                foreach (FileInfo candidatFirstFile in ret)
                {
                    if (AppCst.FirstFilePatternRegex.IsMatch(candidatFirstFile.Name))
                    {
                        FirstFile = candidatFirstFile;
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    Console.WriteLine("Attente");
                    Thread.Sleep(2000);
                }
               
            }

            Match m = AppCst.FilePatternRegex.Match(FirstFile.Name);
            if (!m.Success)
            {
                // ERRROR
                Console.WriteLine("Erreur : '{0}' n'est pas un fichier valide pour commencer.", FirstFile.FullName);
                return;
            }


            long totalBytesRead = 0;

            long totalBytesToRead = long.Parse(m.Groups["size"].Value);
            String finalFileName = m.Groups["name"].Value;

            int i = int.Parse(m.Groups["part"].Value) + 1;


            FileInfo targetFile = new FileInfo(Path.Combine(Target, "~" + finalFileName));

            using (FileStream fo = new FileStream(targetFile.FullName, FileMode.Create))
            {
                FileInfo currentFileToRead = FirstFile;

                while (totalBytesRead < totalBytesToRead)
                {

                    while (!currentFileToRead.Exists)
                    {
                        fo.Flush();
                        Console.WriteLine(".");
                        Thread.Sleep(2000);
                        currentFileToRead.Refresh();
                    }

                    using (FileStream fr = new FileStream(currentFileToRead.FullName, FileMode.Open))
                    {

                        byte[] buffer = new byte[AppCst.BufferSize];
                        int bytesRead;

                        while ((bytesRead = fr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            //localBytesRead += bytesRead;

                            fo.Write(buffer, 0, bytesRead);
                        }

                    }


                    currentFileToRead = new FileInfo(Path.Combine(FirstFile.Directory.FullName, FileUtils.GetFileName(finalFileName, totalBytesToRead, i++)));


                }


           


            }

            targetFile.MoveTo(Path.Combine(Target, finalFileName));


            Console.WriteLine("Terminé");
            return;
        }
    }
}
