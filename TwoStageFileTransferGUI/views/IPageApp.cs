using TwoStageFileTransferCore.dto;

namespace TwoStageFileTransferGUI.views
{
    public interface IPageApp
    {
        bool CanGoNext(AppArgs appArgs, out IPageApp nextPageApp);

        void Navigate(AppArgs appArg, bool isNavigateBack = false);

        void UpdArgsAndGotoPrevious(AppArgs appArgs);
        MainWindow.IActionsWindow MainWindow { get; set; }

    }
}