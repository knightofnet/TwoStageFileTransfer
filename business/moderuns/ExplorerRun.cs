﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business.moderuns
{
    public class ExplorerRun : AbstractModeRun
    {


        public override void InitLogAndAskForParams(AppArgs appArgs)
        {
            if (appArgs.Direction == AppCst.MODE_IN)
            {
              
                if (appArgs.Source == null)
                {
                    appArgs.Source = FileUtils.WinformGetValidFilepath("Select filepath to transfer...");
                }

                if (appArgs.Target == null)
                {
                    appArgs.Target = FileUtils.WinformGetValidDirectorypath("Select filepath to transfer..."); ;
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
                   appArgs.Target = FileUtils.WinformGetValidSaveFilepath("Select filepath to transfer..."); ;
                    
                }
            }
        }
    }
}