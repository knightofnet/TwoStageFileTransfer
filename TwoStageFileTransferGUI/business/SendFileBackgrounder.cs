using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TwoStageFileTransferCore.business.transfer;
using TwoStageFileTransferCore.business.transfer.firststage;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferGUI.constant;
using TwoStageFileTransferGUI.dto;
using TwoStageFileTransferGUI.utils;

namespace TwoStageFileTransferGUI.business
{
    internal class SendFileBackgrounder
    {

        
        public AppArgs AppArgs { get; set; }
        public ProgressBar UiProgressBar { get; set; }

        public Label UiProgressText { get; set; }

        public BackgroundWorker Bck { get; set; }

        public void InitBackgrounder()
        {
            Bck = new BackgroundWorker();
            Bck.WorkerReportsProgress = true;
            Bck.ProgressChanged += BckOnProgressChanged;

            Bck.WorkerSupportsCancellation = true;

            Bck.DoWork += BckOnDoWork;

        }

        private void BckOnDoWork(object sender, DoWorkEventArgs e)
        {

            long maxTransferLenght = AppArgs.MaxDiskPlaceToUse;
            if (maxTransferLenght == -1)
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

            InWorkOptions jobOptions = new InWorkOptions()
            {
                AppArgs = AppArgs,
                MaxSizeUsedOnShared = maxTransferLenght,
            };

            AbstractInWork w = null;
            if (AppArgs.TransferType == TransferTypes.Windows)
            {
                w = new InToOutWork(jobOptions);
            }
            else if (AppArgs.TransferType == TransferTypes.FTP)
            {
               // w = new FtpInWork(_appParams.Connexion, jobOptions);
            }
            else if (AppArgs.TransferType == TransferTypes.SFTP)
            {
             //   w = new SftpInWork(_appParams.Connexion, jobOptions);
            }

            Action<double, string> actionReport = (d, s) =>
            {
                BckgerReportObj reporter = new BckgerReportObj();
                reporter.Text = s;
                reporter.PercentProgressed = (int)d;
                if (double.IsNaN(d))
                {
                    reporter.Type = BckgerReportType.TextOnly;
                }
                Bck.ReportProgress(0, reporter);
            };

            try
            {
               using (AppProgressBar pbar = new AppProgressBar(actionReport))
                    w.DoTransfert(pbar);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void BckOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BckgerReportObj report = e.UserState as BckgerReportObj;
            if (report == null) return;

            UiProgressBar.Value = report.PercentProgressed;
            UiProgressText.Content = report.Text;
        }
    }
}
