using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.dto;

namespace TwoStageFileTransfer.business.transferworkers
{
    internal abstract class AbstractOutWork : AbstractWork
    {
        public OutWorkOptions Options { get; private set; }

        public AbstractOutWork(OutWorkOptions outWorkOptions)
        {
            Options = outWorkOptions;
        }
    }
}
