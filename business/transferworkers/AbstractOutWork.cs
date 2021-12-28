using TwoStageFileTransfer.dto;

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
    }
}
