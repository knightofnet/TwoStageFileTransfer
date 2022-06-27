namespace TwoStageFileTransferCore.dto.transfer
{
    public class InWorkOptions : CommonWorkOptions
    {
        public long MaxSizeUsedOnShared { get; set; }

        public long PartFileSize => AppArgs.ChunkSize;


        public string TsftFilePath { get; set; }
        
    }
}
