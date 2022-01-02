using System;
using System.IO;
using System.Net;

namespace TwoStageFileTransfer.business.connexions
{
    public interface IConnexion
    {
        NetworkCredential Credentials { get; }

        bool DeleteFile(string path);
        bool DeleteFile(Uri path);

        bool CreateDirectory(string directoryPath, bool isRecurseCreate = false);
        bool CreateDirectory(Uri directoryPath, bool isRecurseCreate = false);

        bool DownloadFileFromServer(string path, FileInfo localFile);
        bool DownloadFileFromServer(Uri path, FileInfo localFile);

        bool RenameFile(string path, string newNameFile);
        bool RenameFile(Uri path, string newNameFile);

        bool UploadFileToServer(string directoryPath, FileInfo localFile);
        bool UploadFileToServer(Uri directoryPath, FileInfo localFile);

        bool IsFileExists(string path);
        bool IsFileExists(Uri path);

        bool IsDirectoryExists(string directoryPath);
        bool IsDirectoryExists(Uri directoryPath);

        bool IsOkToConnect();

        void UploadStreamToServer(Stream stream, string fileAbsolutePath, bool canOverride = false, Action<ulong> uploadCallback = null);
        void Close();
    }
}