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
                Console.WriteLine("Erreur : aucun premier fichier saisi");
                return;
            }
            else if ( !Source.Exists && !AryxDevLibrary.utils.FileUtils.IsADirectory(Source.FullName))
            {
                // ERROR
                Console.WriteLine("Erreur : '{0}' n'existe pas", Source.FullName);
                return;
            }

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
                    Console.WriteLine("Wait for transfert files to be generated");
                    Thread.Sleep(2000);
                }
               
            }

            Match m = AppCst.FilePatternRegex.Match(Source.Name);
            if (!m.Success)
            {
                // ERRROR
                Console.WriteLine("Erreur : '{0}' n'est pas un fichier valide pour commencer.", Source.FullName);
                return;
            }


            long totalBytesRead = 0;

            long totalBytesToRead = long.Parse(m.Groups["size"].Value);
            String finalFileName = m.Groups["name"].Value;

            int i = int.Parse(m.Groups["part"].Value) + 1;


            FileInfo targetFile = new FileInfo(Path.Combine(Target, "~" + finalFileName));

            using (FileStream fo = new FileStream(targetFile.FullName, FileMode.Create))
            {
                FileInfo currentFileToRead = Source;

                while (totalBytesRead < totalBytesToRead)
                {

                    while (!currentFileToRead.Exists || AryxDevLibrary.utils.FileUtils.IsFileLocked(currentFileToRead))
                    {
                        fo.Flush();
                        Console.Title = String.Format("TSFT - Out - {0} de {1}", "En attente", currentFileToRead.FullName);
                        Thread.Sleep(500);
                        currentFileToRead.Refresh();
                    }

                    String msg = "Lecture du fichier " + currentFileToRead.Name;
                    Console.Title = String.Format("TSFT - Out - {0}", msg);
                    Console.Write(msg);

                    using (FileStream fr = new FileStream(currentFileToRead.FullName, FileMode.Open))
                    {

                        byte[] buffer = new byte[BufferSize];
                        int bytesRead;

                        while ((bytesRead = fr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            //localBytesRead += bytesRead;

                            fo.Write(buffer, 0, bytesRead);
                        }

                    }

                    msg = " [OK] > Suppression ";
                    Console.WriteLine(msg);

                    currentFileToRead.Delete();


                    currentFileToRead = new FileInfo(Path.Combine(Source.Directory.FullName, FileUtils.GetFileName(finalFileName, totalBytesToRead, i++)));


                }

            }

            targetFile.MoveTo(Path.Combine(Target, finalFileName));


            Console.WriteLine("Terminé");
            return;
        }
    }
}
