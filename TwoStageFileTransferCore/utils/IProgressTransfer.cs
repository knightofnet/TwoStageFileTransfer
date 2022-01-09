using System;

namespace TwoStageFileTransferCore.utils
{
    public interface IProgressTransfer: IDisposable
    {
        void Report(double value, string text);

        void Init();
    }
}