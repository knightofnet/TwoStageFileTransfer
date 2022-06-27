using System.IO;

namespace TwoStageFileTransferCore.dto.transfer
{
    public class CommonWorkOptions
    {
        public AppArgs AppArgs { get; set; }
        public FileInfo Source => new FileInfo(AppArgs.Source);
        public string Target => AppArgs.Target;

        public int BufferSize => AppArgs.BufferSize;

        public bool CanOverwrite => AppArgs.CanOverwrite;

        public int StartPart => AppArgs.ResumePart;

        public string TsftPassphrase => AppArgs.TsftPassphrase;

        public double NbMinWaitForFreeSpace { get; set; }

        public CommonWorkOptions()
        {
            NbMinWaitForFreeSpace = 5;
        }
    }
}
