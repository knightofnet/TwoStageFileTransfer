namespace TwoStageFileTransferCore.dto.transfer
{
    public class OutWorkOptions : CommonWorkOptions
    {
        public TsftFile Tsft => AppArgs.TsftFile;

        public bool KeepPartFiles => AppArgs.KeepPartFiles;
    }
}
