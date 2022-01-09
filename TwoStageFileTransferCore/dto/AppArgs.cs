using TwoStageFileTransferCore.constant;

namespace TwoStageFileTransferCore.dto
{
    public class AppArgs
    {
        public DirectionTrts Direction { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }

        public int BufferSize { get;  set; }

        public long MaxDiskPlaceToUse { get; set; }
        public long ChunkSize { get;  set; }
        public bool CanOverwrite { get; set; }
        
        public TransferTypes TransferType { get; set; }

        public bool IsRemoteTransfertType => TransferType == TransferTypes.FTP || TransferType == TransferTypes.SFTP;
        public string FtpUser { get; set; }

        public string FtpPassword { get; set; }
        public TsftFile TsftFile { get; set; }
        public int ResumePart { get; set; }
        public bool IncludeCredsInTsft { get; set; }
        public bool KeepPartFiles { get;  set; }
        public CredentialOrigins CredentialsOrigin { get; set; }
        public string RemoteHost { get;  set; }
        public int RemotePort { get;  set; }
        public string TsftPassphrase { get; set; }
      

        public AppArgs()
        {
            BufferSize = AppCst.BufferSize;

        }



        



    }
}
