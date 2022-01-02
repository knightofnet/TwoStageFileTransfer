using System.IO;

namespace TwoStageFileTransfer.dto
{
    class CommonWorkOptions
    {
        public AppArgs AppArgs { get; internal set; }
        public FileInfo Source => new FileInfo(AppArgs.Source);
        public string Target => AppArgs.Target;

        public int BufferSize => AppArgs.BufferSize;

        public bool CanOverwrite => AppArgs.CanOverwrite;

        public int StartPart => AppArgs.ResumePart;

        public string TsftPassphrase => AppArgs.TsftPassphrase;
    }
}
