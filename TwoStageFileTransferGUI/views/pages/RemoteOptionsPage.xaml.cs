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
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.utils;

namespace TwoStageFileTransferGUI.views.pages
{
    /// <summary>
    /// Logique d'interaction pour RemoteOptionsPage.xaml
    /// </summary>
    public partial class RemoteOptionsPage : Page, IPageApp
    {
        public MainWindow.IActionsWindow MainWindow { get; set; }
        public RemoteOptionsPage()
        {
            InitializeComponent();
        }

        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {

        }

        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            throw new NotImplementedException();
        }



        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            throw new NotImplementedException();
        }


    }
}
