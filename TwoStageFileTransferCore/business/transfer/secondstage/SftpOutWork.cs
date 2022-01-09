using System;
using System.IO;
using System.Linq;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferCore.business.transfer.secondstage
{
    public class SftpOutWork : FtpOutWork
    {
        public SftpOutWork(IConnexion connexion, OutWorkOptions outWorkOptions) : base(connexion, outWorkOptions)
        {

        }

        protected override long ReadPartFile(AppFileFtp currentFileToRead, long totalBytesRead, FileStream fo, IProgressTransfer transfertReporter, long totalBytesToRead)
        {
            string msg = "Reading file " + currentFileToRead.File.Segments.Last();
            Console.Title = $"TSFT - Out - {msg}";
            _log.Debug(msg);

         

            DateTime localStart = DateTime.Now;
            long currentFoPos = fo.Position;

            var read = totalBytesRead;
            ((SshConnexion)Connexion).DownloadFromServer(fo, currentFileToRead.File.AbsolutePath, bytesRead =>
            {

                transfertReporter.Report((double)((long)bytesRead + read) / totalBytesToRead,
                    CommonAppUtils.GetTransferSpeed((long)bytesRead, localStart));
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
