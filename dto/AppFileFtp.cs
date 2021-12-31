using System;
using System.Net;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.business.connexions;

namespace TwoStageFileTransfer.dto
{
    class AppFileFtp
    {
        public Uri File { get; }

        public Uri FileTemp { get; }

        public Uri DirectoryParent { get; }
        public long Length { get; internal set; }

        


        public AppFileFtp(string parent, string filename)
        {
            File = UriUtils.NewFtpUri(FtpUtils.FtpPathCombine(parent, filename));
            FileTemp = UriUtils.NewFtpUri(FtpUtils.FtpPathCombine(parent, "~" + filename));

            DirectoryParent = UriUtils.NewFtpUri(parent);

            

        }

        public bool Exists(IConnexion connexion)
        {
            return connexion.IsFileExists(File.AbsoluteUri);
        }


        public void MoveToNormal(IConnexion connexion)
        {
            if (!connexion.RenameFile(FileTemp.AbsolutePath, File.Segments[File.Segments.Length - 1]))
            {
                throw new Exception("Error when renaming file on FTP");
            }
        }

        public void Delete(IConnexion connexion)
        {
            if (!connexion.DeleteFile(File.AbsolutePath))
            {
                throw new Exception("Error when deleting file on FTP");
            }
        }
    }
}
