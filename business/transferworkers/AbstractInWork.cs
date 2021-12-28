using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using AFileUtils = AryxDevLibrary.utils.FileUtils;

namespace TwoStageFileTransfer.business.transferworkers
{
    internal abstract class AbstractInWork : AbstractWork
    {
        public int LastPartDone { get; set; }
        public InWorkOptions InWorkOptions { get; private set; }

        public AbstractInWork(InWorkOptions inWorkOptions)
        {
            InWorkOptions = inWorkOptions;
        }

        public abstract void DoTransfert();

        protected abstract long CalculatePartFileMaxLenght();






    }
}
