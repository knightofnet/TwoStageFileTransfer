using System.IO;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.exceptions;

namespace TwoStageFileTransfer.business.transferworkers
{
    internal abstract class AbstractOutWork : AbstractWork
    {
        public OutWorkOptions Options { get; }



        protected AbstractOutWork(OutWorkOptions outWorkOptions)
        {
            Options = outWorkOptions;
        }

        public abstract void DoTransfert();


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
                    throw new AppException(
                        $"Target file '{Options.Target}' already exists. Use parameter -{AppArgsParser.OptCanOverwrite.ShortOpt} to allow overwriting.",
                        EnumExitCodes.KO_CHECK_BEFORE_TRT);
                }
            }
        }
    }
}
