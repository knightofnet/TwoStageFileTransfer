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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.utils;

namespace TwoStageFileTransferGUI.views.pages
{
    /// <summary>
    /// Logique d'interaction pour SendFilePage.xaml
    /// </summary>
    public partial class SendFilePage : Page, IPageApp
    {
        public MainWindow.IActionsWindow MainWindow { get; set; }

        public SendFilePage()
        {
            InitializeComponent();
            Background = null;
        }

        public void Navigate(AppArgs appArg, bool isNavigateBack = false)
        {
            MainWindow.TogglePreviousButton(false);
            MainWindow.ToggleNextButton(false, "Terminer");

            SendFile(appArg);
        }


        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {

            nextPageApp = null;
            return false;

        }



        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }



        private void SendFile(AppArgs appArg)
        {
            MainWindow.SendFile(appArg, pbarSend, lblSentDetail);
        }

    }
}
