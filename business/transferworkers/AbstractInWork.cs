using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.dto;

namespace TwoStageFileTransfer.business.transferworkers
{
    internal abstract class AbstractInWork : AbstractWork
    {
        public InWorkOptions InWorkOptions { get; private set; }

        public AbstractInWork(InWorkOptions inWorkOptions)
        {
            InWorkOptions = inWorkOptions;
        }

       

    }
}
