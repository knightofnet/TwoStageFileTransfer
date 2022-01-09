using System;
using System.Collections.Generic;
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
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.utils;

namespace TwoStageFileTransferGUI.views.pages
{
    /// <summary>
    /// Logique d'interaction pour TplPage.xaml
    /// </summary>
    public partial class SendFileOptionsPage : Page, IPageApp
    {
        private TransferTypes _currentSelected = TransferTypes.None;
        public MainWindow.IActionsWindow MainWindow { get; set; }
        public SendFileOptionsPage()
        {
            InitializeComponent();
            Background = null;
        }

        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {
            MainWindow.TogglePreviousButton(true);

            switch (appArg.TransferType)
            {
                case TransferTypes.Windows:
                    gpSendWindowsParams.IsEnabled = true;
                    rbByWindows.IsChecked = true;
                    break;
                case TransferTypes.FTP:
                    gpSendWindowsParams.IsEnabled = false;
                    rbByFTP.IsChecked = true;
                    break;
                case TransferTypes.SFTP:
                    gpSendWindowsParams.IsEnabled = false;
                    rbBySFTP.IsChecked = true;
                    break;
            }
            _currentSelected = appArg.TransferType;

            if (!string.IsNullOrEmpty(appArg.Target))
            {
                tboxFilePath.Text = appArg.Target;
            }

            rbByWindows.Click += AdaptUiAtMode;
            rbByFTP.Click += AdaptUiAtMode;
            rbBySFTP.Click += AdaptUiAtMode;

            MainWindow.ToggleNextButton(Directory.Exists(tboxFilePath.Text), "Envoyer");

            tboxFilePath.LostFocus += (sender, args) =>
            {

                String filepath = tboxFilePath.Text;
                MainWindow.ToggleNextButton(Directory.Exists(filepath), "Envoyer");
            };


        }

        private void AdaptUiAtMode(object sender, RoutedEventArgs e)
        {
            gpSendWindowsParams.IsEnabled = rbByWindows.IsChecked ?? false;

            if (rbByWindows.IsChecked ?? false)
            {
                _currentSelected = TransferTypes.Windows;
            }
            else if (rbByFTP.IsChecked ?? false)
            {
                _currentSelected = TransferTypes.FTP;
            }
            else if (rbBySFTP.IsChecked ?? false)
            {
                _currentSelected = TransferTypes.SFTP;
            }
        }

        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            if (_currentSelected == TransferTypes.Windows)
            {
                String dirPath = tboxFilePath.Text;
                if (String.IsNullOrEmpty(dirPath))
                {
                    MessageBox.Show("Veuillez entrer l'emplacement du dossier partagé.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    tboxFilePath.Focus();

                    nextPageApp = null;
                    return false;
                }
                else if (!Directory.Exists(dirPath))
                {
                    MessageBox.Show($"Le dossier partagé '{dirPath}' n'existe pas ou n'est pas accessible.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    tboxFilePath.Focus();

                    nextPageApp = null;
                    return false;
                }

                nextPageApp = new SendFilePage();
            }
            else
            {
                nextPageApp = new RemoteOptionsPage();
            }

            return true;
        }

        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }



    }
}
