using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.constant;
using TwoStageFileTransferCore.dto;

namespace TwoStageFileTransfer.dto
{
    internal class CmdAppArgs : AppArgs
    {

        public SourceRuns SourceRun { get; set; }

        public bool IsRunByCmd()
        {
            return SourceRun == SourceRuns.CommandPrompt;
        }

        public bool IsRunByExplorer()
        {
            return SourceRun == SourceRuns.Explorer;
        }
    }
}
