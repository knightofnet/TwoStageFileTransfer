using TwoStageFileTransfer.dto;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransfer.business.moderuns
{
    public class ExplorerRun : AbstractModeRun
    {


        public override void InitLogAndAskForParams(AppArgs appArgs)
        {
            if (appArgs.Direction == DirectionTrts.IN)
            {
              
                if (appArgs.Source == null)
                {
                    appArgs.Source = FileUtils.WinformGetValidFilepath("Select filepath to transfer...");
                }

                if (appArgs.Target == null)
                {
                    appArgs.Target = FileUtils.WinformGetValidDirectorypath("Select filepath to transfer..."); 
                }
            }
            else if (appArgs.Direction == DirectionTrts.OUT)
            {
               
                if (appArgs.Source == null)
                {
                    appArgs.Source = FileUtils.ConsoleGetValidFilepath("Enter source folder to transfer for, or path to tstr file: ");
                }

                if (appArgs.Target == null)
                {
                   appArgs.Target = FileUtils.WinformGetValidDirectorypath("Select filepath to transfer..."); 
                    
                }
            }
        }
    }
}
