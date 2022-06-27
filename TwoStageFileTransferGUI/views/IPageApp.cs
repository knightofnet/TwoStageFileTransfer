using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.dto;

namespace TwoStageFileTransferGUI.views
{
    public interface IPageApp
    {
        MainWindow.IActionsWindow MainWindow { get; set; }
        PageProperties PageProperties { get; set; }


        bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp);

        void Navigate(AppArgs appArgs, bool isNavigateBack = false);

        void UpdArgsAndGotoPrevious(AppArgs appArgs);
       

        void LoadSettingsPage(IPageSettings genPs);
    }
}