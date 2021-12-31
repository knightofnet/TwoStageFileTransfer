using System;
using System.IO;
using System.Net;

namespace TwoStageFileTransfer.business.connexions
{
    public interface IConnexion
    {
        NetworkCredential Credentials { get; }

        bool DeleteFile(string path);

        bool CreateDirectory(string directoryPath, bool isRecurseCreate = false);

        bool DownloadFileFromServer(string path, FileInfo localFile);

        bool RenameFile(string path, string newNameFile);

        bool UploadFileToServer(string directoryPath, FileInfo localFile);

        bool IsFileExists(string path);

        bool IsDirectoryExists(string directoryPath);

        bool IsOkToConnect();

        Stream GetUploadStream(string remoteFilePath);
    }
}