using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.business.transfer;
using TwoStageFileTransferCore.business.transfer.firststage;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.exceptions;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferCore.utils.events;
using TwoStageFileTransferGUI.dto;
using TwoStageFileTransferGUI.utils;

namespace TwoStageFileTransferGUI.business
{
    internal class SendFileBackgrounder
    {


        public AppArgs AppArgs { get; set; }

        public Action<object, ProgressChangedEventArgs> BckEvent { get; set; }

        public Action<Exception> OnError { get; set; }






        public BackgroundWorker Bck { get; set; }

        public void InitBackgrounder()
        {
            Bck = new BackgroundWorker();
            Bck.WorkerReportsProgress = true;
            Bck.ProgressChanged += BckOnBckEventRaised;

            Bck.WorkerSupportsCancellation = true;

            Bck.RunWorkerCompleted += BckOnRunWorkerCompleted;


            Bck.DoWork += BckOnDoWork;

        }


        private void BckOnDoWork(object sender, DoWorkEventArgs e)
        {

            long maxTransferLenght = AppArgs.MaxDiskPlaceToUse;
            if (maxTransferLenght <= 0)
            {
                if (AppArgs.TransferType == TransferTypes.Windows)
                {
                    maxTransferLenght = (long)(FileUtils.GetAvailableSpace(AppArgs.Target, 20 * 1024 * 1024) * 0.9);
                    //I(_log, $"Max size that can be used: {AryxDevLibrary.utils.FileUtils.HumanReadableSize(maxTransferLenght)}");
                }
                else if (AppArgs.IsRemoteTransfertType)
                {
                    maxTransferLenght = (long)AryxDevLibrary.utils.FileUtils.HumanReadableSizeToLong("20Mo");
                }
            }

            if (AppArgs.ChunkSize <= 0)
            {
                AppArgs.ChunkSize = -1;
            }

            if (string.IsNullOrWhiteSpace(AppArgs.TsftPassphrase))
            {
                AppArgs.TsftPassphrase = AppWords.GetNWords(4).Aggregate((c, s) => $"{c} {s}");
            }

            InWorkOptions jobOptions = new InWorkOptions()
            {
                AppArgs = AppArgs,
                MaxSizeUsedOnShared = maxTransferLenght,
                NbMinWaitForFreeSpace = 60,
                
                
            };

            AbstractInWork w = null;
            switch (AppArgs.TransferType)
            {
                case TransferTypes.Windows:
                    w = new InToOutWork(jobOptions);
                    break;

                case TransferTypes.FTP:
                    {
                        UriBuilder uriBuilder = new UriBuilder();
                        uriBuilder.Scheme = "ftp";
                        uriBuilder.Host = AppArgs.RemoteHost;
                        uriBuilder.Port = AppArgs.RemotePort;

                        IConnexion con = new FtpConnexion(
                            new NetworkCredential(AppArgs.FtpUser, AppArgs.FtpPassword),
                            uriBuilder.Uri);

                        w = new FtpInWork(con, jobOptions);
                        break;

                    }
                case TransferTypes.SFTP:
                    //IConnexion con = new SFtpConnexion();
                    //w = new SftpInWork(con.Connexion, jobOptions);
                    break;

            }

            try
            {
                using (AppProgressBar pbar = new AppProgressBar())
                {
                    pbar.TsftFileCreated += WorkTsftFileCreated;
                    pbar.Progress += WorkReportProgress;
                    pbar.CheckIsCanceled += () => Bck.CancellationPending;


                    w.DoTransfert(pbar);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void WorkTsftFileCreated(object sender, TsftFileCreatedArgs args)
        {
            BckgerReportObj reporter = new BckgerReportObj
            {
                Type = BckgerReportType.TsftFileCreated,
                Object = args
            };

            Bck.ReportProgress(int.MinValue, reporter);
        }

        private void WorkReportProgress(string text, double percent, BckgerReportType type)
        {
            BckgerReportObj reporter = new BckgerReportObj
            {
                Text = text,
                PercentProgressed = percent,
                Type = type
            };

            Bck.ReportProgress(int.MinValue, reporter);
        }

        private void BckOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                BckgerReportObj reporter = new BckgerReportObj
                {
                    Text = "Transfert terminé",
                    PercentProgressed = 100,
                    Type = BckgerReportType.Finished
                };

                BckEvent?.Invoke(sender, new ProgressChangedEventArgs(int.MinValue, reporter));
            }
            else
            {

                OnError?.Invoke(e.Error);

            }
        }

        private void BckOnBckEventRaised(object sender, ProgressChangedEventArgs e)
        {

            BckEvent?.Invoke(sender, e);
        }
    }
}
