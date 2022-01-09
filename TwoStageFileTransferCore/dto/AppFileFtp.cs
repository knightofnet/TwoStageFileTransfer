using System;
using AryxDevLibrary.utils;
using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferCore.dto
{
    public class AppFileFtp
    {
        public Uri File { get; }

        public Uri FileTemp { get; }

        public Uri DirectoryParent { get; }
        public long Length { get; set; }

        


        public AppFileFtp(string parent, string filename)
        {
            File = FtpUtils.FtpPathCombine(parent, filename).ToUri();
            FileTemp = FtpUtils.FtpPathCombine(parent, "~" + filename).ToUri();

            DirectoryParent = parent.ToUri();

        }

        public bool Exists(IConnexion connexion, bool isTempFileToCheck = false)
        {
            if (isTempFileToCheck)
            {
                return connexion.IsFileExists(FileTemp);
            }
            return connexion.IsFileExists(File);

        }


        public void MoveToNormal(IConnexion connexion)
        {
            if (!connexion.RenameFile(FileTemp.AbsolutePath, File.Segments[File.Segments.Length - 1]))
            {
                throw new Exception("Error when renaming file on FTP");
            }
        }

        public void Delete(IConnexion connexion, bool isTempFileToDelete = false)
        {
            Uri fileUri = File;
            if (isTempFileToDelete)
            {
                fileUri = FileTemp;
            }

            if (!connexion.DeleteFile(fileUri))
            {
                throw new Exception("Error when deleting file on FTP");
            }
        }
    }
}
