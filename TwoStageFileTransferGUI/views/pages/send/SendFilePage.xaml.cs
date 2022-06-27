using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AryxDevLibrary.utils;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.utils.events;
using TwoStageFileTransferGUI.dto;

namespace TwoStageFileTransferGUI.views.pages.send
{
    /// <summary>
    /// Logique d'interaction pour SendFilePage.xaml
    /// </summary>
    public partial class SendFilePage : Page, IPageApp
    {
        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }

        private AppArgs _appArgs;
        

        public SendFilePage()
        {
            InitializeComponent();
            Background = null;

            gridTsftFileCreated.Visibility = Visibility.Collapsed;
            gpTransfer.Visibility = Visibility.Collapsed;

            lblSentDetail.Content = "";
        }

        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {
            
            MainWindow.ToggleNextButton(false, "Terminer");

            _appArgs = appArg;

            lblSourceFile.Content = appArg.Source;
            lblSourceSize.Content = FileUtils.HumanReadableSize(new FileInfo(appArg.Source).Length);
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

        private void OnBckChanged(object s, ProgressChangedEventArgs e)
        {
            if (!(e.UserState is BckgerReportObj report)) return;

            switch (report.Type)
            {
                case BckgerReportType.ProgressPbarText:
                case BckgerReportType.ProgressPbarOnly:
                case BckgerReportType.ProgressTextOnly:
                    OnProgressChanged(s, report);
                    break;

                case BckgerReportType.TsftFileCreated:
                    if (report.Object is TsftFileCreatedArgs tsftFileCreatedArgs)
                    {
                        OnTsftfileCreated(s, tsftFileCreatedArgs);
                    }

                    break;
                case BckgerReportType.Finished:
                    MainWindow.ToggleNextButton(true, "Terminer");
                    break;
            }

            ;






        }

        private void OnTransferError( Exception e)
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

        public void OnTsftfileCreated(object sender, TsftFileCreatedArgs args)
        {
            gridTsftFileCreated.Visibility = Visibility.Visible;
            hlinkOpenFolderTsft.Click += (o, eventArgs) =>
            {
                FileUtils.ShowFileInWindowsExplorer(args.Filepath);
            };
            tboxPassPhrase.Text = args.Passphrase;
        }



        private void SendFile(AppArgs appArg)
        {

            MainWindow.SendFile(appArg, OnBckChanged, OnTransferError);
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                MoreSendOptionsView mv = new MoreSendOptionsView();
                mv.LoadsWith(_appArgs);
                if (mv.ShowDialog() ?? false)
                {
                    mv.UpdateObj(_appArgs);
                }

                return;
            }

            gpTransfer.Visibility = Visibility.Visible;
            if (MainWindow.IsInTransfert())
            {
                MainWindow.StopAction();
                btnStart.IsEnabled = false;
            }
            else
            {
                SendFile(_appArgs);
                MainWindow.TogglePreviousButton(false);
                btnStart.Content = "Stopper le transfert";
            }

        }
    }
}
