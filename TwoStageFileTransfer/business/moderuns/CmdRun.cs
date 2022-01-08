using AryxDevLibrary.utils.logger;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransfer.business.moderuns
{
    public class CmdRun : AbstractModeRun
    {
        private static Logger _log;

        public override void InitLogAndAskForParams(AppArgs appArgs)
        {
            if (appArgs.Direction == DirectionTrts.IN)
            {
                if (appArgs.Source == null)
                {
                    appArgs.Source = FileUtils.ConsoleGetValidFilepath("Enter the source file path: ");
                }

                if (appArgs.Target == null)
                {
                    appArgs.Target = FileUtils.ConsoleGetValidDirectorypath("Enter target directory to transfer the part files: ");
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
                    appArgs.Target = FileUtils.ConsoleGetValidDirectorypath("Enter target file to recompose part file: ");
                }
            }
        }
    }
}
