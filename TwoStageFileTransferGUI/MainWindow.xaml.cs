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
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.business;
using TwoStageFileTransferGUI.utils;
using TwoStageFileTransferGUI.views;
using TwoStageFileTransferGUI.views.pages;


namespace TwoStageFileTransferGUI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, MainWindow.IActionsWindow
    {
        private static Logger _log;
        private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwoStageFileTransfer");


        public interface IActionsWindow
        {
            void ToggleNextButton(bool state, string label = "Suivant");
            void TogglePreviousButton(bool state);
            void SetTitle(string title);
            void SendFile(AppArgs appArg, ProgressBar pbar, Label lbl);
        }

        private readonly AppArgs appArgs = new AppArgs();



        public MainWindow()
        {
            InitializeComponent();
            mainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            if (!Directory.Exists(AppDataDir))
            {
                Directory.CreateDirectory(AppDataDir);
            }
            _log = new Logger(Path.Combine(AppDataDir, "logWin.log"), Logger.LogLvl.NONE, Logger.LogLvl.DEBUG, "1 Mo");


            appArgs.Direction = DirectionTrts.IN;
            appArgs.TransferType = TransferTypes.Windows;
            appArgs.Source = @"C:\Users\ARyx\Desktop\destination\GitHubDesktopSetup-x64.exe";
            appArgs.Target = @"C:\Users\ARyx\Desktop\destination\shared";
            appArgs.ChunkSize = -1;
            appArgs.MaxDiskPlaceToUse = (long)FileUtils.HumanReadableSizeToLong("200Mo");
            appArgs.TsftPassphrase = "TEST";
            

            mainFrame.Navigating += (sender, args) =>
            {
                if (args.Content is IPageApp page)
                {
                    page.Navigate(appArgs, args.NavigationMode == NavigationMode.Back);
                }
            };

            IPageApp choicePage = new ChoiceTrtPage();
            choicePage.MainWindow = this;

            mainFrame.Navigate(choicePage);


        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (!mainFrame.CanGoBack || !(mainFrame.Content is IPageApp currentPageApp)) return;

            Console.WriteLine(currentPageApp.ToString());
            mainFrame.GoBack();


        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            IPageApp currentPageApp = mainFrame.Content as IPageApp;
            if (currentPageApp == null) return;

            if (currentPageApp.CanGoNext(appArgs, out var nextPageApp))
            {
                nextPageApp.MainWindow = this;
                mainFrame.Navigate(nextPageApp);
            }

        }

        public void ToggleNextButton(bool state, string label = "Suivant")
        {
            btnNext.IsEnabled = state;
            btnNext.Content = label;
        }

        public void TogglePreviousButton(bool state)
        {
            btnPrev.IsEnabled = state;
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        public void SendFile(AppArgs appArg, ProgressBar pbar, Label lbl)
        {
            SendFileBackgrounder sfb = new SendFileBackgrounder
            {
                AppArgs = appArg,
                UiProgressBar = pbar,
                UiProgressText = lbl
            };
            sfb.InitBackgrounder();

            sfb.Bck.RunWorkerAsync();

        }
    }
}
