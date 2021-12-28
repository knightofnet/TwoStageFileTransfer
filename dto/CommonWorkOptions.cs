using System.IO;

namespace TwoStageFileTransfer.dto
{
    class CommonWorkOptions
    {
        public FileInfo Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }

        public bool CanOverwrite { get; internal set; }

        public int StartPart { get; set; }
    }
}
