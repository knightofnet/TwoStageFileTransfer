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
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.dto;

namespace TwoStageFileTransferGUI.views.pages.shared
{
    /// <summary>
    /// Logique d'interaction pour ChooseFolderPage.xaml
    /// </summary>
    public partial class ChooseFolderPage : Page, IPageApp
    {
        public class ChooseFolderPageSettings : IPageSettings
        {
            public string PageTitle { get; set; }
            public string PageDescription { get; set; }

            public string ErrorMsgIsNullOrEmptyFile { get; set; }

            public Func<AppArgs, string> TboxFileValue { get; set; }

            public Func<AppArgs, string, bool> TreatmentCanGoNext { get; set; }

        }

        public MainWindow.IActionsWindow MainWindow { get; set; }
        public PageProperties PageProperties { get; set; }


        public ChooseFolderPageSettings CurrentPageSettings { get; set; }

        public ChooseFolderPage()
        {
            InitializeComponent();
            Background = null;
        }



        public void LoadSettingsPage(IPageSettings genPs)
        {
            if (!(genPs is ChooseFolderPageSettings ps)) return;

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
            
            tboxFilePath.Text = CurrentPageSettings.TboxFileValue?.Invoke(appArg) ?? string.Empty;

            MainWindow.ToggleNextButton(File.Exists(tboxFilePath.Text));

            tboxFilePath.TextChanged += (sender, args) =>
            {
                string filepath = tboxFilePath.Text;
                bool isOk = File.Exists(filepath);
                MainWindow.ToggleNextButton(isOk);
                lblClickNext.Visibility = isOk ? Visibility.Visible : Visibility.Collapsed;
            };

        }

        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            string folderPath = tboxFilePath.Text;
            if (string.IsNullOrEmpty(folderPath))
            {
                MessageBox.Show(CurrentPageSettings.ErrorMsgIsNullOrEmptyFile,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                tboxFilePath.Focus();

                nextPageApp = null;
                return false;
            }

            if (!Directory.Exists(folderPath.Trim('"')))
            {
                MessageBox.Show($"Le dossier '{folderPath}' n'existe pas ou n'est pas accessible.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                tboxFilePath.Focus();

                nextPageApp = null;
                return false;
            }



            if (CurrentPageSettings.TreatmentCanGoNext != null &&
                !CurrentPageSettings.TreatmentCanGoNext.Invoke(appArgs, tboxFilePath.Text))
            {

                nextPageApp = null;
                return false;
            }

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
            CommonOpenFileDialog openFolderDialog = new CommonOpenFileDialog
            {
                Multiselect = false,
                IsFolderPicker = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFolderDialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            tboxFilePath.Text = openFolderDialog.FileName;
            MainWindow.ToggleNextButton(true);
        }



    }
}
