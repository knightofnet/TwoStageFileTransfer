using System.IO;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.exceptions;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferCore.business.transfer
{
    public abstract class AbstractOutWork : AbstractWork
    {
        public OutWorkOptions Options { get; }
        
        protected AbstractOutWork(OutWorkOptions outWorkOptions)
        {
            Options = outWorkOptions;
        }

        public abstract void DoTransfert(IProgressTransfer transferReporter);


        protected void TestFileAlreadyExists(FileInfo rTargetFile)
        {
            if (rTargetFile.Exists)
            {
                if (Options.CanOverwrite)
                {
                    _log.Warn("{0} already exists : delete");
                    rTargetFile.Delete();
                    rTargetFile.Refresh();
                }
                else
                {
                    throw new CommonAppException(
                        $"Target file '{Options.Target}' already exists. Use parameter -{CmdArgsOptions.OptCanOverwrite.ShortOpt} to allow overwriting.",
                        CommonAppExceptReason.ErrorPreparingTreatment);
                }
            }
        }
    }
}
