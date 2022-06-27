using System;
using System.Windows.Controls;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferGUI.dto;
using TwoStageFileTransferGUI.views.pages.send;

namespace TwoStageFileTransferGUI.views.pages.shared
{
    /// <summary>
    /// Logique d'interaction pour RemoteOptionPathPage.xaml
    /// </summary>
    public partial class RemoteOptionPathPage : Page, IPageApp
    {
        private AppArgs _appArgs;

        private readonly UriBuilder uriBuilder;

        public RemoteOptionPathPage()
        {
            InitializeComponent();
            Background = null;
            uriBuilder = new UriBuilder();
        }

        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }

        public void Navigate(AppArgs appArgs, bool isNavigateBack = false)
        {
            _appArgs = appArgs;

            MainWindow.TogglePreviousButton(true);
            MainWindow.ToggleNextButton(true);


            uriBuilder.Scheme = "ftp";
            if (_appArgs.TransferType == TransferTypes.SFTP)
            {
                uriBuilder.Scheme = "sftp";
            }

            uriBuilder.Port = _appArgs.RemotePort;
            uriBuilder.Host = _appArgs.RemoteHost;

            if (_appArgs.RemotePort > 0)
            {
                uriBuilder.Port = _appArgs.RemotePort;
            }
            lblFtp.Content = uriBuilder.Uri.AbsoluteUri;

            tboxFilePath.TextChanged += (sender, args) =>
            {
                uriBuilder.Path = tboxFilePath.Text;
                string schemeComplete = uriBuilder.Uri.AbsoluteUri;
                lblUri.Content = schemeComplete;
            };
        }

        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            appArgs.Target = lblUri.Content as string;

            PageProperties nextPageProperties = MainWindow.ArianeTrtPath.NextPageProperties(PageProperties.Name, appArgs);
            nextPageApp = nextPageProperties.CreateNextPageInstance();
            nextPageApp.PageProperties = nextPageProperties;

            return true;
        }

        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            appArgs.Target = lblUri.Content as string;
        }

        public void LoadSettingsPage(IPageSettings genPs)
        {
            //throw new NotImplementedException();
        }
    }
}
