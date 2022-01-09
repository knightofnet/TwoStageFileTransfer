using AryxDevLibrary.utils.logger;

namespace TwoStageFileTransferCore.business.transfer
{
    public abstract class AbstractWork
    {
        protected static Logger _log = Logger.LastLoggerInstance ;

        /*
        public FileInfo Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }

        public bool CanOverwrite { get;internal set; }
        */
    }
}
