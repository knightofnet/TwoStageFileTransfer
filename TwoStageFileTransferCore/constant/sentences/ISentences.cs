using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransferCore.constant.sentences
{
    public interface ISentences
    {
        string TargetDirectoryNotExists { get; }
        string InTransfertResuming { get; }

        string InTransfertCalcFileHash { get; }
        string InTransfertRetryWrite { get; }
        string InTransfertHeader { get; }
        string InTransfertCreatePartFile { get; }
        string InTransfertWaitingOutFreeSpace { get; }
        string InTransfertWaitingOutFreeSpaceDetails { get; }
    }
}
