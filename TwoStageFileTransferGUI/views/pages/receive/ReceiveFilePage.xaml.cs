using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AryxDevLibrary.utils;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.utils.events;
using TwoStageFileTransferGUI.dto;

namespace TwoStageFileTransferGUI.views.pages.receive
{
    /// <summary>
    /// Logique d'interaction pour ReceiveFilePage.xaml
    /// </summary>
    public partial class ReceiveFilePage : Page, IPageApp
    {


        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }

        private AppArgs _appArgs;

        public ReceiveFilePage()
        {
            InitializeComponent();
            Background = null;

            //gridTsftFileCreated.Visibility = Visibility.Collapsed;
            gpTransfer.Visibility = Visibility.Collapsed;

            lblSentDetail.Content = "";
        }

        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {

            MainWindow.ToggleNextButton(false, "Terminer");

            _appArgs = appArg;

            lblSourceFile.Content = appArg.TsftFile.TempDir.Path;
            lblSourceSize.Content = FileUtils.HumanReadableSize(appArg.TsftFile.FileLenght);
            lblModeTransfert.Content = appArg.TransferType;
            lblTarget.Content = appArg.Target;

            //SendFile(appArg);
        }


        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {

            MainWindow.EndSession(appArgs);

            nextPageApp = null;
            return false;

        }



        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }

        public void LoadSettingsPage(IPageSettings genPs)
        {
            //throw new NotImplementedException();
        }

        private void OnBckChanged(object s, BckgerReportObj report)
        {


            switch (report.Type)
            {
                case BckgerReportType.ProgressPbarText:
                case BckgerReportType.ProgressPbarOnly:
                case BckgerReportType.ProgressTextOnly:
                    OnProgressChanged(s, report);
                    break;
                case BckgerReportType.Finished:
                    MainWindow.ToggleNextButton(true, "Terminer");
                    break;
            }

        }

        private void ReceiveFile(AppArgs appArg)
        {

            MainWindow.ReceiveFile(appArg, OnBckChanged, OnTransferError);
        }

        private void OnTransferError(Exception e)
        {
            pbarSend.IsIndeterminate = true;
            pbarSend.Foreground = Brushes.DarkRed;

            MessageBox.Show($"Une erreur inattendue est survenue durant le transfert : {e.Message}", "Erreur",
                MessageBoxButton.OK, MessageBoxImage.Error);

            lblSentDetail.Content = e.Message;
        }

        private void OnProgressChanged(object s, BckgerReportObj report)
        {
            if (report.Type == BckgerReportType.ProgressPbarText || report.Type == BckgerReportType.ProgressPbarOnly)
            {
                if (!double.IsNaN(report.PercentProgressed))
                {
                    pbarSend.IsIndeterminate = false;
                    pbarSend.Value = report.PercentProgressed;
                }
                else
                {
                    pbarSend.IsIndeterminate = true;
                }
            }
            else if (report.Type == BckgerReportType.ProgressPbarText || report.Type == BckgerReportType.ProgressPbarOnly ||
                     report.Type == BckgerReportType.ProgressTextOnly)
            {
                lblSentDetail.Content = report.Text;
            }
        }







        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            gpTransfer.Visibility = Visibility.Visible;

            ReceiveFile(_appArgs);
            MainWindow.TogglePreviousButton(false);
            //btnStart.Content = "Stopper le transfert";


        }
    }
}
