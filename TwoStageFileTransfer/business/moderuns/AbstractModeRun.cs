using TwoStageFileTransfer.dto;
using TwoStageFileTransferCore.dto;

namespace TwoStageFileTransfer.business.moderuns
{
    public abstract class AbstractModeRun
    {

        public abstract void InitLogAndAskForParams(AppArgs appArgs);
    }
}
