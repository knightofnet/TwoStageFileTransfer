namespace TwoStageFileTransfer.dto
{
    class InWorkOptions : CommonWorkOptions
    {
        public long MaxSizeUsedOnShared { get; set; }

        public long PartFileSize { get; set; }
        public string TsftFile { get; set; }
        
    }
}
