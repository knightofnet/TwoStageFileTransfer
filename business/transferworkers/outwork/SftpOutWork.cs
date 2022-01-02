using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.business.connexions;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business.transferworkers.outwork
{
    class SftpOutWork : FtpOutWork
    {
        public SftpOutWork(IConnexion connexion, OutWorkOptions outWorkOptions) : base(connexion, outWorkOptions)
        {

        }

        protected override long ReadPartFile(AppFileFtp currentFileToRead, long totalBytesRead, FileStream fo, ProgressBar pbar, long totalBytesToRead)
        {
            string msg = "Reading file " + currentFileToRead.File.Segments.Last();
            Console.Title = $"TSFT - Out - {msg}";
            _log.Debug(msg);

         

            DateTime localStart = DateTime.Now;
            long currentFoPos = fo.Position;

            var read = totalBytesRead;
            ((SshConnexion)Connexion).DownloadFromServer(fo, currentFileToRead.File.AbsolutePath, bytesRead =>
            {
              
                pbar.Report((double)((long)bytesRead + read) / totalBytesToRead,
                    AppUtils.GetTransferSpeed((long)bytesRead, localStart));
            });

            totalBytesRead += fo.Position - currentFoPos;
            
            _log.Debug("> OK");
            if (!Options.KeepPartFiles)
            {
                currentFileToRead.Delete(Connexion);
                _log.Debug("> File part deleted");
            }

            return totalBytesRead;
        }


    }
}
