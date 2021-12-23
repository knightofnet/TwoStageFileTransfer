using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business.moderuns
{
    public class CmdRun : AbstractModeRun
    {
        private static Logger _log;

        public override void InitLogAndAskForParams(AppArgs appArgs)
        {
            if (appArgs.Direction == AppCst.MODE_IN)
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
            else if (appArgs.Direction == AppCst.MODE_OUT)
            {
                if (appArgs.Source == null)
                {
                    appArgs.Source = FileUtils.ConsoleGetValidFilepath("Enter source folder to transfer for, or path to tstr file: ");
                }

                if (appArgs.Target == null)
                {
                    appArgs.Target = FileUtils.ConsoleGetValidFilepath("Enter target file to recompose part file: ");
                }
            }
        }
    }
}
