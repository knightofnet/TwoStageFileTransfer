using System;
using System.Xml.Serialization;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.utils;
using TwoStageFileTransferCore.constant;

namespace TwoStageFileTransfer.dto
{

    [Serializable]
    public class TsftFile
    {

        public SourceInfo Source { get; set; }



        //public TargetInfo Target { get; private set; }



        public TempDirInfo TempDir { get;  set; }

        public string Sha1Hash { get; set; }

        public long FileLenght { get; set; }

        

        public TsftFile()
        {
            Source = new SourceInfo();
            //Target = new TargetInfo();
            TempDir = new TempDirInfo();
        }



        public class SourceInfo
        {
            public string OriginalDirectory { get; set; }

            public string OriginalFilename { get; set; }

            

        }
        public class TempDirInfo
        {
            [XmlAttribute]
            public TransferTypes Type { get; set; }

            public string Path { get; set; }

            public long RegularPartFileLenght { get; set; }

            public string FtpUsername { get; set; }

            public string FtpPassword { get; set; }

            [XmlIgnore]
            public Uri FtpPath => Path?.ToUri();

            public long AwaitedParts { get; set; }
            public string RemoteHost { get; set; }
            public int RemotePort { get; set; }
        }

        /*
        internal class TargetInfo
        {

        }
        */

    }
}
