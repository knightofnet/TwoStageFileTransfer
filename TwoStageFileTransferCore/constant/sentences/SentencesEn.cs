using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransferCore.constant.sentences
{
    public class SentencesEn : ISentences
    {

        public String TargetDirectoryNotExists { get; } = "Directory '{0}' not found and impossible to create.";
        public string InTransfertResuming { get; } = "Resuming";
        public string InTransfertCalcFileHash { get; } = "Computing file hash";
        public string InTransfertRetryWrite { get; } = "Retry";
        public string InTransfertCreatePartFile { get; } = "Creating part file {0}";

        public string InTransfertHeader { get; } = "TSFT - In - {0}";

        public string InTransfertWaitingOutFreeSpace { get; } =
            "TSFT - In - Waiting for OUT mode to work and freeing disk space...";

        public string InTransfertWaitingOutFreeSpaceDetails { get; } =
            "Waiting for OUT mode to work and freeing disk space : {0} + {1} > {2}";
    }
}
