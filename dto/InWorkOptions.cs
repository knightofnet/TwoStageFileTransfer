namespace TwoStageFileTransfer.dto
{
    class InWorkOptions : CommonWorkOptions
    {
        public long MaxSizeUsedOnShared { get; set; }

        public long PartFileSize => AppArgs.ChunkSize;


        public string TsftFilePath { get; set; }
        
    }
}
