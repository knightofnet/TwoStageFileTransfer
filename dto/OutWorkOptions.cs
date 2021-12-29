namespace TwoStageFileTransfer.dto
{
    class OutWorkOptions : CommonWorkOptions
    {
        public TsftFile Tsft => AppArgs.TsftFile;

        public bool KeepPartFiles => AppArgs.KeepPartFiles;
    }
}
