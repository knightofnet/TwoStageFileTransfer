using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.dto;
using TwoStageFileTransferGUI.views.pages.shared;

namespace TwoStageFileTransferGUI.views.pages.send
{
    /// <summary>
    /// Logique d'interaction pour TplPage.xaml
    /// </summary>
    public partial class SendFileOptionsPage : Page, IPageApp
    {
        private TransferTypes _currentSelected = TransferTypes.None;
        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }

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

            MainWindow.ToggleNextButton(Directory.Exists(tboxFilePath.Text));

            tboxFilePath.LostFocus += AdaptUiAtMode;


        }


        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            if (_currentSelected == TransferTypes.Windows)
            {
                string dirPath = tboxFilePath.Text?.Trim('"');
                if (string.IsNullOrEmpty(dirPath))
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

                appArgs.Target = dirPath;
                appArgs.TransferType = TransferTypes.Windows;
               
            }
            else 
            {
                appArgs.TransferType = _currentSelected;
              
            }

            PageProperties nextPageProperties = MainWindow.ArianeTrtPath.NextPageProperties(PageProperties.Name, appArgs);
            nextPageApp = nextPageProperties.CreateNextPageInstance();
            nextPageApp.PageProperties = nextPageProperties;


            return true;
        }

        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }

        public void LoadSettingsPage(IPageSettings genPs)
        {
            //throw new NotImplementedException();
        }


        private void AdaptUiAtMode(object sender, RoutedEventArgs e)
        {
            gpSendWindowsParams.IsEnabled = rbByWindows.IsChecked ?? false;
            MainWindow.ToggleNextButton(true);

            if (rbByWindows.IsChecked ?? false)
            {
                _currentSelected = TransferTypes.Windows;

                string filepath = tboxFilePath.Text?.Trim('"');
                MainWindow.ToggleNextButton(Directory.Exists(filepath), "Envoyer");

                lblClickNext.Visibility = Visibility.Visible;

            }
            else if (rbByFTP.IsChecked ?? false)
            {
                _currentSelected = TransferTypes.FTP;
                MainWindow.ToggleNextButton(true);
                lblClickNext.Visibility = Visibility.Collapsed;
            }
            else if (rbBySFTP.IsChecked ?? false)
            {
                _currentSelected = TransferTypes.SFTP;
                MainWindow.ToggleNextButton(true);
                lblClickNext.Visibility = Visibility.Collapsed;
            }
        }

        private void btnBrowseForAfile_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFolderDialog = new CommonOpenFileDialog
            {
                Multiselect = false,
                IsFolderPicker = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFolderDialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            tboxFilePath.Text = openFolderDialog.FileName;
            AdaptUiAtMode(null, null);
        }
    }
}
