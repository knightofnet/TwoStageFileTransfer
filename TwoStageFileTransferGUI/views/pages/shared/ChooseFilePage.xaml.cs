using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.dto;

namespace TwoStageFileTransferGUI.views.pages.shared
{
    /// <summary>
    /// Logique d'interaction pour ChooseFilePage.xaml
    /// </summary>
    public partial class ChooseFilePage : Page, IPageApp
    {
        public class PageSettings : IPageSettings
        {
            public string PageTitle { get; set; }
            public string PageDescription { get; set; }

            public string ErrorMsgIsNullOrEmptyFile { get; set; }

            public string BrowseFileFilter { get; set; }

            public Func<string> TboxFileValue { get; set; }

            public Func<AppArgs, string, bool> TreatmentCanGoNext { get; set; }

        }

        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }


        public PageSettings CurrentPageSettings { get; set; }

        public ChooseFilePage()
        {
            InitializeComponent();
            Background = null;
        }



        public void LoadSettingsPage(IPageSettings genPs)
        {
            if (!(genPs is PageSettings ps)) return;

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
            MainWindow.TogglePreviousButton(true);



            tboxFilePath.Text = CurrentPageSettings.TboxFileValue.Invoke();

            MainWindow.ToggleNextButton(File.Exists(tboxFilePath.Text));

            tboxFilePath.TextChanged += (sender, args) =>
            {
                String filepath = tboxFilePath.Text;
                bool isOk = File.Exists(filepath);
                MainWindow.ToggleNextButton(isOk);
                lblClickNext.Visibility = isOk ? Visibility.Visible : Visibility.Collapsed;
            };

        }

        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            String filepath = tboxFilePath.Text;
            if (String.IsNullOrEmpty(filepath))
            {
                MessageBox.Show(CurrentPageSettings.ErrorMsgIsNullOrEmptyFile,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                tboxFilePath.Focus();

                nextPageApp = null;
                return false;
            }
            else if (!File.Exists(filepath.Trim('"')))
            {
                MessageBox.Show($"Le fichier '{filepath}' n'existe pas ou n'est pas accessible.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                tboxFilePath.Focus();

                nextPageApp = null;
                return false;
            }

            filepath = filepath.Trim('"');

            if (CurrentPageSettings.TreatmentCanGoNext != null &&
                !CurrentPageSettings.TreatmentCanGoNext.Invoke(appArgs, tboxFilePath.Text))
            {

                nextPageApp = null;
                return false;
            }

            appArgs.Source = filepath;

            PageProperties nextPageProperties = MainWindow.ArianeTrtPath.NextPageProperties(PageProperties.Name, appArgs);
            nextPageApp = nextPageProperties.CreateNextPageInstance();
            nextPageApp.LoadSettingsPage(nextPageProperties.PageSettings);
            nextPageApp.PageProperties = nextPageProperties;

            return true;

        }

        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }

        private void btnBrowseForAfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = CurrentPageSettings.BrowseFileFilter,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() != true) return;

            tboxFilePath.Text = openFileDialog.FileName;
            MainWindow.ToggleNextButton(true);
        }


  
    }
}
