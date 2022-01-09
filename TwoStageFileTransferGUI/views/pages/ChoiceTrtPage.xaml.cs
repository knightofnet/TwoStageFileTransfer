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
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;

namespace TwoStageFileTransferGUI.views.pages
{
    /// <summary>
    /// Logique d'interaction pour ChoiceTrtPage.xaml
    /// </summary>
    public partial class ChoiceTrtPage : Page, IPageApp
    {
        public MainWindow.IActionsWindow MainWindow { get; set; }


        public ChoiceTrtPage()
        {
            InitializeComponent();
            Background = null;

        }


        public void Navigate(AppArgs appArg, bool isNavigateBack=false)
        {
            MainWindow.TogglePreviousButton(false);
            MainWindow.ToggleNextButton(appArg.Direction != DirectionTrts.NONE);

            switch (appArg.Direction)
            {
                case DirectionTrts.IN:
                    rbSendFile.IsChecked = true;
                    break;
                case DirectionTrts.OUT:
                    rbReceiveFile.IsChecked = true;
                    break;
            }
        }


        public bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp)
        {
            nextPageApp = null;
            if (rbSendFile.IsChecked ?? false)
            {
                appArgs.Direction = DirectionTrts.IN;
                nextPageApp = new ChooseFilePage();
            } else if (rbReceiveFile.IsChecked ?? false)
            {
                appArgs.Direction = DirectionTrts.OUT;
                nextPageApp = null;
            }

            
            return true;
        }

        public void UpdArgsAndGotoPrevious(AppArgs appArgs)
        {
            //throw new NotImplementedException();
        }

        
    }
}
