using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils;

namespace TwoStageFileTransfer.dto
{
    class AppFileFtp
    {
        public Uri File { get; }

        public Uri FileTemp { get; }

        public Uri DirectoryParent { get; }
        public long Length { get; internal set; }

        private readonly ICredentials credentials;


        public AppFileFtp(string parent, string filename, ICredentials credentials)
        {
            File = FtpUtils.NewFtpUri(FtpUtils.FtpPathCombine(parent, filename));
            FileTemp = FtpUtils.NewFtpUri(FtpUtils.FtpPathCombine(parent, "~" + filename));

            DirectoryParent = FtpUtils.NewFtpUri(parent);

            this.credentials = credentials;

        }

        public bool Exists()
        {
            return FtpUtils.IsFileExists(File, credentials);
        }


        public void MoveToNormal()
        {
            if (!FtpUtils.RenameFile(FileTemp, File.Segments[File.Segments.Length - 1], credentials))
            {
                throw new Exception("Error when renaming file on FTP");
            }
        }

        public void Delete()
        {
            if (!FtpUtils.DeleteFile(File, credentials))
            {
                throw new Exception("Error when deleting file on FTP");
            }
        }
    }
}
