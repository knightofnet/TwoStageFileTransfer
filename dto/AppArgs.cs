﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.constant;

namespace TwoStageFileTransfer.dto
{
    public class AppArgs
    {
        public string Direction { get; internal set; }
        public string Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }

        public bool IsDoCompress { get; internal set; }
        public long ChunkSize { get; internal set; }
        public bool CanOverwrite { get; set; }
        public SourceRuns SourceRun { get; set; }

        public AppArgs()
        {
            BufferSize = AppCst.BufferSize;
            
        }

        public bool IsRunByCmd()
        {
            return SourceRun == SourceRuns.CommandPrompt;
        }

        public bool IsRunByExplorer()
        {
            return SourceRun == SourceRuns.Explorer; ;
        }

        
    }
}
