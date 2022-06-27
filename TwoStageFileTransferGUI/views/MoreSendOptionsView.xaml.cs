using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using AryxDevLibrary.utils;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.utils;
using UsefulCsharpCommonsUtils.file;
using UsefulCsharpCommonsUtils.lang;
using UsefulCsharpCommonsUtils.ui.linker;

namespace TwoStageFileTransferGUI.views
{
    /// <summary>
    /// Logique d'interaction pour MoreSendOptionsView.xaml
    /// </summary>
    public partial class MoreSendOptionsView : Window, IUiLinker<AppArgs>
    {
        private UiLink<AppArgs> appArgsUiLink;

        public MoreSendOptionsView()
        {
            InitializeComponent();
            LoadsWith(new AppArgs());
        }

        public void LoadsWith(AppArgs obj)
        {
            appArgsUiLink = new UiLink<AppArgs>(obj);

            appArgsUiLink.AddCustumBinding(tboxMaxFileSize, "MaxDiskPlaceToUse",
                    (a, e) =>
                    {
                        long valueLong = a.MaxDiskPlaceToUse;
                        ((TextBox)e).Text = CommonsFileUtils.HumanReadableSize(valueLong);
                        return valueLong.ToString();
                    },
                    (e, a) =>
                    {
                        double valueDbl = CommonsFileUtils.HumanReadableSizeToLong(((TextBox)e).Text);
                        if (!Double.IsNaN(valueDbl))
                        {
                            a.MaxDiskPlaceToUse = (long)valueDbl;
                        }
                        else
                        {
                            MessageBox.Show("La taille est incorrecte", "Erreur", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            e.Focus();
                        }

                        return ((TextBox)e).Text;
                    }
                );
            appArgsUiLink.AddCustumBinding(tboxMaxFilePartSize, "ChunkSize",
                (a, e) =>
                {
                    long valueLong = a.ChunkSize;
                    ((TextBox)e).Text = CommonsFileUtils.HumanReadableSize(valueLong);
                    return valueLong.ToString();
                },
                (e, a) =>
                {
                    double valueDbl = CommonsFileUtils.HumanReadableSizeToLong(((TextBox)e).Text);
                    if (!Double.IsNaN(valueDbl))
                    {
                        a.ChunkSize = (long)valueDbl;
                    }
                    else
                    {
                        MessageBox.Show("La taille est incorrecte", "Erreur", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        e.Focus();
                    }

                    return ((TextBox)e).Text;
                }
            );

            appArgsUiLink.AddBindingCheckbox(chkOverwriteShared, "CanOverwrite");

            appArgsUiLink.DoRead();
        }

        public AppArgs UpdateObj(AppArgs enviro)
        {
            AppArgs obj = enviro ?? appArgsUiLink.Object;

            appArgsUiLink.DoUpdate(obj);

            return obj;


        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
