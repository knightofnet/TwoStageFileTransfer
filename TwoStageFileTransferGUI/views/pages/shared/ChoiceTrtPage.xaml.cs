using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.cliParser;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferGUI.dto;
using TwoStageFileTransferGUI.utils;
using UsefulCsharpCommonsUtils.ui;
using UsefulCsharpCommonsUtils.ui.inputbox;

namespace TwoStageFileTransferGUI.views.pages.shared
{
    /// <summary>
    /// Logique d'interaction pour ChoiceTrtPage.xaml
    /// </summary>
    public partial class ChoiceTrtPage : Page, IPageApp
    {
        public class ChoiceTrtPageSettings : IPageSettings
        {
            public string PageTitle { get; set; }
            public string PageDescription { get; set; }
            public Func<AppArgs, string, bool> TreatmentCanGoNext { get; set; }

        }

        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }

        public ChoiceTrtPageSettings CurrentPageSettings { get; set; }


        public ChoiceTrtPage()
        {
            InitializeComponent();
            Background = null;

        }

        public void LoadSettingsPage(IPageSettings genPs)
        {
            if (!(genPs is ChoiceTrtPageSettings ps)) return;

            if (ps.PageTitle != null)
            {
                lblPageTitle.Content = ps.PageTitle;
            }

            if (ps.PageDescription != null)
            {
                lblTxtDescription.Content = ps.PageDescription;
            }

            CurrentPageSettings = ps;
        }




        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {
            MainWindow.TogglePreviousButton(false);
            MainWindow.ToggleNextButton(appArg.Direction != DirectionTrts.NONE);

            switch (appArg.Direction)
            {
                case DirectionTrts.IN:
                    rbSendFile.IsChecked = true;
                    break;
                case DirectionTrts.OUT:
                    rbReceiveFile.IsChecked = true;
                    break;
            }
        }


        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            nextPageApp = null;
            if (rbSendFile.IsChecked ?? false)
            {
                appArgs.Direction = DirectionTrts.IN;

                PageProperties nextPageProperties = MainWindow.ArianeTrtPath.NextPageProperties(PageProperties.Name, appArgs);
                nextPageApp = nextPageProperties.CreateNextPageInstance();
                nextPageApp.LoadSettingsPage(nextPageProperties.PageSettings);
                nextPageApp.PageProperties = nextPageProperties;

            }
            else if (rbReceiveFile.IsChecked ?? false)
            {
                appArgs.Direction = DirectionTrts.OUT;

                PageProperties nextPageProperties = MainWindow.ArianeTrtPath.NextPageProperties(PageProperties.Name, appArgs);
                nextPageApp = nextPageProperties.CreateNextPageInstance();
                nextPageProperties.PageSettings.TreatmentCanGoNext = OpenTsftAndGoNext;
                nextPageApp.LoadSettingsPage(nextPageProperties.PageSettings);
                nextPageApp.PageProperties = nextPageProperties;
            }


            return true;
        }

        private bool OpenTsftAndGoNext(AppArgs appArgs, string tsftFilePath)
        {


            if (appArgs.TsftPassphrase == null)
            {
                appArgs.TsftPassphrase = AppCst.DefaultPassPhrase;
            }

            TsftFile tsftFile = CommonAppUtils.DecryptTsft(appArgs, tsftFilePath, false);
            while (tsftFile == null)
            {

                if (InputBoxView.InputBox("Entrez la phrase de passe pour déchiffrer le fichier TSFT :", "Déchiffrer le fichier TSFT", out string passphraseInput))
                {
                    appArgs.TsftPassphrase = passphraseInput;
                }
                else
                {
                    return false;
                }
                tsftFile = CommonAppUtils.DecryptTsft(appArgs, tsftFilePath, false);

            }

            switch (tsftFile.TempDir.Type)
            {
                case TransferTypes.FTP:
                    appArgs.TransferType = TransferTypes.FTP;
                    break;
                case TransferTypes.SFTP:
                    appArgs.TransferType = TransferTypes.SFTP;
                    break;
                case TransferTypes.Windows:
                    appArgs.TransferType = TransferTypes.Windows;
                    break;
            }

            appArgs.TsftFile = tsftFile;
            if (string.IsNullOrWhiteSpace(appArgs.FtpUser))
            {
                appArgs.CredentialsOrigin = CredentialOrigins.TsftFile;
                appArgs.FtpUser = appArgs.TsftFile.TempDir.FtpUsername;
                appArgs.FtpPassword = appArgs.TsftFile.TempDir.FtpPassword;
            }

            appArgs.RemotePort = appArgs.TsftFile.TempDir.RemotePort;
            appArgs.RemoteHost = appArgs.TsftFile.TempDir.Path;
            


            return true;


        }

        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }


    }
}
