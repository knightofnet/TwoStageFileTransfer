using TwoStageFileTransfer.constant;
using TwoStageFileTransferCore.constant;

namespace TwoStageFileTransfer.dto
{
    public class AppArgs
    {
        public DirectionTrts Direction { get; internal set; }
        public string Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }

        public bool IsDoCompress { get; internal set; }

        public long MaxDiskPlaceToUse { get; set; }
        public long ChunkSize { get; internal set; }
        public bool CanOverwrite { get; set; }
        public SourceRuns SourceRun { get; set; }
        public TransferTypes TransferType { get; set; }

        public bool IsRemoteTransfertType => TransferType == TransferTypes.FTP || TransferType == TransferTypes.SFTP;
        public string FtpUser { get; set; }

        public string FtpPassword { get; set; }
        public TsftFile TsftFile { get; set; }
        public int ResumePart { get; set; }
        public bool IncludeCredsInTsft { get; set; }
        public bool KeepPartFiles { get; internal set; }
        public CredentialOrigins CredentialsOrigin { get; set; }
        public string RemoteHost { get; internal set; }
        public int RemotePort { get; internal set; }
        public string TsftPassphrase { get; set; }
      

        public AppArgs()
        {
            BufferSize = AppCst.BufferSize;

        }

        public bool IsRunByCmd()
        {
            return SourceRun == SourceRuns.CommandPrompt;
        }

        public bool IsRunByExplorer()
        {
            return SourceRun == SourceRuns.Explorer;
        }

        



    }
}
