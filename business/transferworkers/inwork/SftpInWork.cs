using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using TwoStageFileTransfer.business.connexions;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business.transferworkers.inwork
{
    class SftpInWork : FtpInWork
    {
        public SftpInWork(IConnexion connexion, InWorkOptions inWorkOptions) : base(connexion, inWorkOptions)
        {
        }

        protected override long WritePartFile(long chunk, ProgressBar pbar, FileStream fs, AppFileFtp fileOutPath, DateTime localStart)
        {
            long localBytesRead = 0;

            try
            {

                ScpClient scpClient = new ScpClient();

                using (Stream fo = new MemoryStream())
                {


                    string msg = "Creating part file " + fileOutPath.FileTemp.LocalPath;
                    Console.Title = $"TSFT - In - {msg}";
                    _log.Debug(msg);

                    Array.Clear(_buffer, 0, _buffer.Length);
                    int bytesRead;

                    while ((bytesRead = fs.Read(_buffer, 0, _buffer.Length)) > 0)
                    {
                        _totalBytesRead += bytesRead;
                        localBytesRead += bytesRead;

                        fo.Write(_buffer, 0, bytesRead);
                        pbar.Report((double)_totalBytesRead / fs.Length, AppUtils.GetTransferSpeed(localBytesRead, localStart));


                        if (localBytesRead + InWorkOptions.BufferSize > chunk ||
                            localBytesRead + InWorkOptions.BufferSize > InWorkOptions.MaxSizeUsedOnShared)
                        {
                            break;
                        }
                    }

                    fo.Position = 0;


                    scpClient.Upload(fo, fileOutPath.FileTemp.AbsolutePath);

                    fileOutPath.Length = localBytesRead;

                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    _log.Debug($"Upload File Complete, status {response.StatusDescription}");
                    if (response.StatusCode != FtpStatusCode.ClosingData)
                    {
                        throw new Exception($"Error during transfer of {fileOutPath.File.LocalPath}");
                    }
                }


                fileOutPath.MoveToNormal(_connexion);
            }
            catch (Exception ex)
            {
                _log.Debug($"Error while uploading : {ex.Message}");

                if (fileOutPath.Exists(_connexion))
                {
                    fileOutPath.Delete(_connexion);

                }

                throw ex;
            }

            return localBytesRead;
        }
    }
    }
}
