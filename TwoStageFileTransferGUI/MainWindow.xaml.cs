using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.business.transfer;
using TwoStageFileTransferCore.business.transfer.secondstage;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferCore.utils.events;
using TwoStageFileTransferGUI.business;
using TwoStageFileTransferGUI.dto;
using TwoStageFileTransferGUI.utils;
using TwoStageFileTransferGUI.views;
using TwoStageFileTransferGUI.views.pages;
using TwoStageFileTransferGUI.views.pages.receive;
using TwoStageFileTransferGUI.views.pages.send;
using TwoStageFileTransferGUI.views.pages.shared;
using UsefulCsharpCommonsUtils.file;
using UsefulCsharpCommonsUtils.work.backgroundworker;


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
            ArianeTrtPath ArianeTrtPath { get; }

            void ToggleNextButton(bool state, string label = "Suivant");
            void TogglePreviousButton(bool state);
            void SetTitle(string title);
            void SendFile(AppArgs appArg, Action<object, ProgressChangedEventArgs> onBckEvent, Action<Exception> onErrorRaised);
            void StopAction();
            void EndSession(AppArgs appArg);
            bool IsInTransfert();
            void ReceiveFile(AppArgs appArg, EventHandler<BckgerReportObj> onBckChanged, Action<Exception> onTransferError);
        }

        private readonly AppArgs appArgs = new AppArgs();
        private SendFileBackgrounder currentAction;

        public ArianeTrtPath ArianeTrtPath { get; }

        public MainWindow()
        {
            InitializeComponent();
            mainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;

            ArianeTrtPath = new ArianeTrtPath();

            if (!Directory.Exists(AppDataDir))
            {
                Directory.CreateDirectory(AppDataDir);
            }
            _log = new Logger(Path.Combine(AppDataDir, "logWin.log"), Logger.LogLvl.NONE, Logger.LogLvl.DEBUG, "1 Mo");


            appArgs.Direction = DirectionTrts.IN;
            appArgs.TransferType = TransferTypes.Windows;

            if (File.Exists("creds.ini"))
            {
                Ini iniCreds = new Ini("creds.ini");
                string section = "general";

                string maxDiskRaw = iniCreds.GetValue("MaxDiskPlaceToUse", section);
                if (!string.IsNullOrWhiteSpace(maxDiskRaw))
                {
                    appArgs.MaxDiskPlaceToUse = (long)CommonsFileUtils.HumanReadableSizeToLong(maxDiskRaw);
                }

                appArgs.FtpUser = iniCreds.GetValue("FtpUser", section);
                appArgs.FtpPassword = iniCreds.GetValue("FtpPassword", section);
                appArgs.RemoteHost = iniCreds.GetValue("RemoteHost", section);
                appArgs.Target = iniCreds.GetValue("Target", section);

                string remotePortRaw = iniCreds.GetValue("RemotePort", section);
                if (!string.IsNullOrWhiteSpace(maxDiskRaw) && int.TryParse(remotePortRaw, out int remotePortInt))
                {
                    appArgs.RemotePort = remotePortInt;
                }
            }


            mainFrame.Navigating += (sender, args) =>
            {
                if (args.Content is IPageApp page)
                {
                    page.Navigate(appArgs, args.NavigationMode == NavigationMode.Back);
                }
            };

            ConstructPath();

            IPageApp choicePage = new ChoiceTrtPage();
            choicePage.PageProperties = ArianeTrtPath.PathPages.FirstOrDefault(r => r.Name == "Choice");
            choicePage.MainWindow = this;



            mainFrame.Navigate(choicePage);


        }

        private void ConstructPath()
        {

            // Choice -> InChoiceFile
            // Choice -> OutChoiceFile
            PageProperties choiceProperties = new PageProperties()
            {
                Name = "Choice",
                PageType = typeof(ChoiceTrtPage),
                NextDecision = (a) =>
                {
                    if (a.Direction == DirectionTrts.IN)
                    {
                        return "InChoiceFile";
                    }

                    if (a.Direction == DirectionTrts.OUT)
                    {
                        return "OutChoiceFile";
                    }

                    return null;
                }
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            ConstructSendPath();

            ConstructReceivePath();
        }

        private void ConstructReceivePath()
        {
            // OutChoiceFile -> OutFolderTarget
            // OutChoiceFile -> OutRemoteOptions
            PageProperties choiceProperties = new PageProperties()
            {
                Name = "OutChoiceFile",
                PageType = typeof(ChooseFilePage),
                NextDecision = (a) =>
                {

                    return a.TransferType == TransferTypes.Windows
                        ? "OutFolderTarget"
                        : string.IsNullOrWhiteSpace(a.FtpUser) || string.IsNullOrWhiteSpace(a.FtpPassword)
                            ? "OutRemoteOptions"
                            : "OutFolderTarget";
                },
                PageSettings = new ChooseFilePage.PageSettings()
                {
                    BrowseFileFilter = "Tsft files |*.tsft",
                    ErrorMsgIsNullOrEmptyFile = "Veuillez entrer l'emplacement du fichier TSFT.",
                    PageTitle = "Choisissez le fichier TSFT lié au fichier à recevoir",
                    PageDescription = "Choisissez le fichier TSFT contenant les informations du fichier à recevoir.",
                    TboxFileValue = () =>
                    {
                        string source = !string.IsNullOrEmpty(appArgs.Source)
                            ? appArgs.Source
                            : AppUtils.GetValidFilepathFromClipboard();

                        if (string.IsNullOrEmpty(source)) return string.Empty;

                        FileInfo fSource = new FileInfo(source);
                        return fSource.Extension.ToLower().Equals(".tsft") ? source : string.Empty;
                    },
                    //TreatmentCanGoNext = OpenTsftAndGoNext
                }
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // OutRemoteOptions -> OutFolderTarget
            choiceProperties = new PageProperties()
            {
                Name = "OutRemoteOptions",
                PageType = typeof(RemoteOptionsPage),
                NextDecision = a => "OutFolderTarget",
                PageSettings = new RemoteOptionsPage.RemoteOptionsPageSettings()
                {
                    PageTitle = "Informations de connexion au server",
                    PageDescription = "Complêtez les informations de connexion afin de permettre la connexion au server.",
                    CanSaveInTsft = false
                }
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // OutFolderTarget -> ReceiveFile
            choiceProperties = new PageProperties()
            {
                Name = "OutFolderTarget",
                PageType = typeof(ChooseFolderPage),
                NextDecision = (a) => "ReceiveFile",
                PageSettings = new ChooseFolderPage.ChooseFolderPageSettings()
                {
                    ErrorMsgIsNullOrEmptyFile = "Veuillez entrer l'emplacement du dossier à enregistrer.",
                    PageTitle = "Choisissez le dossier où sera enregistré le fichier ?",
                    PageDescription = "Le fichier va être récupéré; choisissez le dossier où le sauvegarder.",
                    TboxFileValue = (a) => a.Target,
                    TreatmentCanGoNext = (args, s) =>
                    {
                        string finalFile = Path.Combine(s, args.TsftFile.Source.OriginalFilename);
                        if (File.Exists(finalFile))
                        {
                            var res = MessageBox.Show(
                                $"La destination comprend déjà un fichier nommé \"{args.TsftFile.Source.OriginalFilename}\". Voulez-vous le remplacer ?",
                                "Remplacer le fichier", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (res == MessageBoxResult.Yes)
                            {
                                File.Delete(finalFile);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        args.Target = s;
                        return true;
                    }
                }
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // ReceiveFile [End]
            choiceProperties = new PageProperties()
            {
                Name = "ReceiveFile",
                PageType = typeof(ReceiveFilePage)
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);
        }

        private void ConstructSendPath()
        {
            // InChoiceFile -> InSendFileOptions
            PageProperties choiceProperties = new PageProperties()
            {
                Name = "InChoiceFile",
                PageType = typeof(ChooseFilePage),
                NextDecision = (a) => "InSendFileOptions",
                PageSettings = new ChooseFilePage.PageSettings()
                {
                    BrowseFileFilter = "All files (*.*)|*.*",
                    ErrorMsgIsNullOrEmptyFile = "Veuillez entrer l'emplacement du fichier à envoyer.",
                    PageTitle = "Pour quel fichier souhaitez-vous faire un envoi ?",
                    PageDescription = "Choisissez le fichier à envoyer",
                    TboxFileValue = () => !string.IsNullOrEmpty(appArgs.Source)
                        ? appArgs.Source
                        : AppUtils.GetValidFilepathFromClipboard()
                }
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // InSendFileOptions -> SendFile
            // InSendFileOptions -> InRemoteOptions
            choiceProperties = new PageProperties()
            {
                Name = "InSendFileOptions",
                PageType = typeof(SendFileOptionsPage),
                NextDecision = (a) => a.TransferType == TransferTypes.Windows ? "SendFile" : "InRemoteOptions",
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // InRemoteOptions -> InRemoteOptionPath
            choiceProperties = new PageProperties()
            {
                Name = "InRemoteOptions",
                PageType = typeof(RemoteOptionsPage),
                NextDecision = (a) => "InRemoteOptionPath",
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // InRemoteOptionPath -> SendFile
            choiceProperties = new PageProperties()
            {
                Name = "InRemoteOptionPath",
                PageType = typeof(RemoteOptionPathPage),
                NextDecision = (a) => "SendFile",
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);

            // SendFile [End]
            choiceProperties = new PageProperties()
            {
                Name = "SendFile",
                PageType = typeof(SendFilePage)
            };
            ArianeTrtPath.PathPages.Add(choiceProperties);
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (!mainFrame.CanGoBack || !(mainFrame.Content is IPageApp currentPageApp)) return;

            Console.WriteLine(currentPageApp.ToString());
            mainFrame.GoBack();

            SetTitle(((IPageApp)mainFrame.Content).PageProperties.Name);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            IPageApp currentPageApp = mainFrame.Content as IPageApp;
            if (currentPageApp == null) return;

            if (currentPageApp.CanGoNext(appArgs, out var nextPageApp))
            {
                SetTitle(currentPageApp.PageProperties.Name);
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

        public void SendFile(AppArgs appArg, Action<object, ProgressChangedEventArgs> onBckEvent, Action<Exception> onErrorRaised)
        {
            currentAction = new SendFileBackgrounder
            {
                AppArgs = appArg,
                BckEvent = onBckEvent,
                OnError = onErrorRaised

            };
            currentAction.InitBackgrounder();

            currentAction.Bck.RunWorkerAsync();

        }

        public void StopAction()
        {
            currentAction?.Bck.CancelAsync();
        }

        public void EndSession(AppArgs appArg)
        {
            Environment.Exit(0);
        }

        public bool IsInTransfert()
        {
            if (currentAction == null || currentAction.Bck == null) return false;

            return currentAction.Bck.IsBusy;
        }

        public void ReceiveFile(AppArgs appArg, EventHandler<BckgerReportObj> onBckChanged, Action<Exception> onErrorRaised)
        {

            WorkBackgrounderWithResult<AppArgs, bool> wk = new WorkBackgrounderWithResult<AppArgs, bool>();
            wk.FinishWithErrorAction = onErrorRaised;
            wk.FinishAction = b =>
            {
                BckgerReportObj reporter = new BckgerReportObj
                {
                    Text = "Terminé",
                    PercentProgressed = 100,
                    Type = BckgerReportType.Finished
                };

                onBckChanged?.Invoke(this, reporter);
            };
            wk.ReportProgressAction = (i, o) =>
            {
                onBckChanged?.Invoke(this, (BckgerReportObj)o);
            };

            wk.WorkAction = (args, worker) =>
            {
                OutWorkOptions jobOptions = new OutWorkOptions()
                {
                    AppArgs = args,
                    NbMinWaitForFreeSpace = 60
                };

                AbstractOutWork o = null;
                switch (appArgs.TransferType)
                {
                    case TransferTypes.Windows:
                        o = new OutToFileWork(jobOptions);
                        break;
                    case TransferTypes.FTP:
                        IConnexion con = new FtpConnexion(
                            new NetworkCredential(args.FtpUser, args.FtpPassword),
                            args.RemoteHost.ToUri());

                        o = new FtpOutWork(con, jobOptions);
                        break;
                    case TransferTypes.SFTP:
                        //o = new SftpOutWork(args.Connexion, jobOptions);
                        break;
                }

                try
                {
                    using (AppProgressBar pbar = new AppProgressBar())
                    {

                        pbar.Progress += (text, percent, type) =>
                        {
                            BckgerReportObj reporter = new BckgerReportObj
                            {
                                Text = text,
                                PercentProgressed = percent,
                                Type = type
                            };

                            worker.ReportProgress(int.MinValue, reporter);
                        };
                        pbar.CheckIsCanceled += () => worker.CancellationPending;


                        o.DoTransfert(pbar);
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }

                return true;

            };


            wk.RunAsync(appArgs);

        }
    }
}
