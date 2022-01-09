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
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.utils;

namespace TwoStageFileTransferGUI.views.pages
{
    /// <summary>
    /// Logique d'interaction pour ChooseFilePage.xaml
    /// </summary>
    public partial class ChooseFilePage : Page, IPageApp
    {
        public MainWindow.IActionsWindow MainWindow { get; set; }

        public ChooseFilePage()
        {
            InitializeComponent();
            Background = null;
        }

        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {
            MainWindow.TogglePreviousButton(true);
            
            

            tboxFilePath.Text = !string.IsNullOrEmpty(appArg.Source) ? appArg.Source : AppUtils.GetValidFilepathFromClipboard();
            
            MainWindow.ToggleNextButton(File.Exists(tboxFilePath.Text));

            tboxFilePath.LostFocus += (sender, args) =>
            {
                String filepath = tboxFilePath.Text;
                MainWindow.ToggleNextButton(File.Exists(filepath));
            };

        }

        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            String filepath = tboxFilePath.Text;
            if (String.IsNullOrEmpty(filepath))
            {
                MessageBox.Show("Veuillez entrer l'emplacement du fichier à envoyer.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                tboxFilePath.Focus();

                nextPageApp = null;
                return false;
            } else if (!File.Exists(filepath))
            {
                MessageBox.Show($"Le fichier '{filepath}' n'existe pas ou n'est pas accessible.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                tboxFilePath.Focus();

                nextPageApp = null;
                return false;
            }

            appArgs.Source = filepath;

            nextPageApp = new SendFileOptionsPage();
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
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() != true) return;

            tboxFilePath.Text = openFileDialog.FileName;
            MainWindow.ToggleNextButton(true);
        }
    }
}
