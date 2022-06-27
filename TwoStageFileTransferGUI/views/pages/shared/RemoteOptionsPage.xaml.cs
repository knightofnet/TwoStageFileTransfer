using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferGUI.dto;
using UsefulCsharpCommonsUtils.lang.ext;

namespace TwoStageFileTransferGUI.views.pages.shared
{
    /// <summary>
    /// Logique d'interaction pour RemoteOptionsPage.xaml
    /// </summary>
    public partial class RemoteOptionsPage : Page, IPageApp
    {
        public class RemoteOptionsPageSettings : IPageSettings
        {
            public string PageTitle { get; set; }
            public string PageDescription { get; set; }

            public bool CanSaveInTsft { get; set; }

            public Func<AppArgs, string, bool> TreatmentCanGoNext { get; set; }

            public RemoteOptionsPageSettings()
            {
                CanSaveInTsft = true;
            }
        }

        public RemoteOptionsPageSettings CurrentPageSettings { get; set; }

        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }

        private AppArgs _appArgs;

        private string _okUsername;
        private string _okPassword;
        private string _okHote;
        private int _okPort;

        public RemoteOptionsPage()
        {
            InitializeComponent();
        }

        public void Navigate(AppArgs appArgs, bool isNavigateBack = false)
        {
            _appArgs = appArgs;
            MainWindow.TogglePreviousButton(true);
            MainWindow.ToggleNextButton(true);

            tbUsername.Text = appArgs.FtpUser;
            tbPassword.Password = appArgs.FtpPassword;
            tbHote.Text = appArgs.RemoteHost;
            tbPort.Text = appArgs.RemotePort + "";

            tbHote.TextChanged += InfoChanged;
            tbPort.TextChanged += InfoChanged;
            tbUsername.TextChanged += InfoChanged;
            tbPassword.PasswordChanged += InfoChanged;


            if (isNavigateBack)
            {
                appArgs.FtpPassword = null;
            }
        }



        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            if (TestConnexion())
            {
                appArgs.FtpUser = _okUsername;
                appArgs.FtpPassword = _okPassword;
                appArgs.RemoteHost = _okHote;
                appArgs.RemotePort = _okPort;

                appArgs.IncludeCredsInTsft = chkSaveInTsftFile.IsChecked ?? false;

                PageProperties nextPageProperties = MainWindow.ArianeTrtPath.NextPageProperties(PageProperties.Name, appArgs);
                nextPageApp = nextPageProperties.CreateNextPageInstance();
                nextPageApp.PageProperties = nextPageProperties;

                tbHote.TextChanged -= InfoChanged;
                tbPort.TextChanged -= InfoChanged;
                tbUsername.TextChanged -= InfoChanged;
                tbPassword.PasswordChanged -= InfoChanged;

                return true;
            }

            nextPageApp = null;
            return false;
        }



        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }

        public void LoadSettingsPage(IPageSettings genPs)
        {
            if (!(genPs is RemoteOptionsPageSettings ps)) return;

            if (ps.PageTitle != null)
            {
                lblPageTitle.Content = ps.PageTitle;
            }

            if (ps.PageDescription != null)
            {
                lblTxtDescription.Content = ps.PageDescription;
            }

            chkSaveInTsftFile.Visibility = ps.CanSaveInTsft ? Visibility.Visible : Visibility.Hidden;

            CurrentPageSettings = ps;
        }




        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            TestConnexion();
        }

        private bool TestConnexion()
        {
            string hoteRaw = tbHote.Text;

            UriBuilder uriBuilder = new UriBuilder();



            IConnexion connexion = null;
            switch (_appArgs.TransferType)
            {
                case TransferTypes.FTP:
                    uriBuilder.Scheme = "ftp";
                    uriBuilder.Port = tbPort.Text.ParseOrDefault(uriBuilder.Port);
                    uriBuilder.Host = hoteRaw.StartsWith("ftp://") ? hoteRaw.Replace("ftp://", string.Empty) : hoteRaw;

                    connexion = new FtpConnexion(
                        new NetworkCredential(tbUsername.Text, tbPassword.SecurePassword),
                        uriBuilder.Uri
                    );
                    break;

                case TransferTypes.SFTP:
                    uriBuilder.Scheme = "sftp";
                    uriBuilder.Port = tbPort.Text.ParseOrDefault(uriBuilder.Port);
                    uriBuilder.Host = hoteRaw.StartsWith("sftp://") ? hoteRaw.Replace("sftp://", string.Empty) : hoteRaw;

                    connexion = new SshConnexion(
                        new NetworkCredential(tbUsername.Text, tbPassword.SecurePassword),
                        hoteRaw, int.Parse(tbPort.Text)
                    );
                    break;
            }

            if (connexion == null) return false;

            if (connexion.IsOkToConnect())
            {
                MessageBox.Show("Test de connexion OK.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);


                _okHote = hoteRaw;
                _okPort = int.Parse(tbPort.Text);
                _okUsername = tbUsername.Text;
                _okPassword = tbPassword.Password;

                return true;
            }
            else
            {
                MessageBox.Show("Test de connexion KO.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private void InfoChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            bool test = IsOkVersusMemory();
            MainWindow.ToggleNextButton(test);

        }

        private bool IsOkVersusMemory()
        {
            if (_okHote != null && !_okHote.Equals(tbHote.Text))
            {
                return false;
            }

            if (_okPort != 0 && _okPort != int.Parse(tbPort.Text))
            {

                return false; ;
            }

            if (_okUsername != null && !_okUsername.Equals(tbUsername.Text))
            {

                return false; ;
            }

            if (_okPassword != null && !_okPassword.Equals(tbPassword.Password))
            {

                return false; ;
            }

            return true;
        }
    }
}
