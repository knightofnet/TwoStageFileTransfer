using System;
using System.IO;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferCore.business.transfer.firststage
{
    public class SftpInWork : FtpInWork
    {
        public SftpInWork(IConnexion connexion, InWorkOptions inWorkOptions) : base(connexion, inWorkOptions)
        {

        }

        protected override long WritePartFile(long chunk, IProgressTransfer reporter, FileStream fs, AppFileFtp fileOutPath, DateTime localStart)
        {
            _log.Info("Using SSH :");

            long localBytesRead = 0;

            try
            {


                using (Stream fo = new MemoryStream())
                {

                    string msg = string.Format(InWorkOptions.Sentences.InTransfertCreatePartFile, fileOutPath.FileTemp.LocalPath);
                    reporter.OnProgress(string.Format(InWorkOptions.Sentences.InTransfertHeader, msg));
                    _log.Debug(msg);

                    Array.Clear(Buffer, 0, Buffer.Length);
                    int bytesRead;

                    while ((bytesRead = fs.Read(Buffer, 0, Buffer.Length)) > 0)
                    {
                        TotalBytesRead += bytesRead;
                        localBytesRead += bytesRead;

                        fo.Write(Buffer, 0, bytesRead);
                        reporter.OnProgress(CommonAppUtils.GetTransferSpeed(localBytesRead, localStart), (double)TotalBytesRead / fs.Length,
                            BckgerReportType.ProgressPbarText);


                        if (localBytesRead + InWorkOptions.BufferSize > chunk ||
                            localBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                        {
                            break;
                        }
                    }

                    fo.Position = 0;

                    Connexion.UploadStreamToServer(fo, fileOutPath.FileTemp.AbsolutePath, InWorkOptions.CanOverwrite,
                        obj =>
                        {
                            reporter.OnProgress(CommonAppUtils.GetTransferSpeed((long)obj, localStart), (double)TotalBytesRead / fs.Length,
                                BckgerReportType.ProgressPbarText);

                        });

                    fileOutPath.Length = localBytesRead;

                }

                fileOutPath.MoveToNormal(Connexion);
            }
            catch (Exception ex)
            {
                _log.Debug($"Error while uploading : {ex.Message}");

                if (fileOutPath.Exists(Connexion))
                {
                    fileOutPath.Delete(Connexion);
                }

                if (fileOutPath.Exists(Connexion, true))
                {
                    fileOutPath.Delete(Connexion, true);
                }

                throw ex;
            }

            return localBytesRead;
        }


        protected override void IncludeMoreThingsInTsftFile(TsftFile file)
        {
            base.IncludeMoreThingsInTsftFile(file);

            file.TempDir.Type = TransferTypes.SFTP;
            if (InWorkOptions.AppArgs.IncludeCredsInTsft)
            {
                file.TempDir.RemoteHost = ((SshConnexion)Connexion).Host;
                file.TempDir.RemotePort = ((SshConnexion)Connexion).Port;
                file.TempDir.FtpPassword = Connexion.Credentials.Password;
            }


        }

    }

}
