using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils;

namespace TwoStageFileTransfer.business.connexions
{
    class FtpConnexion : IConnexion
    {
        public NetworkCredential Credentials { get; }
        public Uri RootUri { get; }
        

        public FtpConnexion(NetworkCredential credential, Uri rootUri)
        {
            Credentials = credential;
            RootUri = rootUri;
        }

        public bool DeleteFile(string path)
        {
            return FtpUtils.DeleteFile(UriWithRoot(path), Credentials);
        }

        public bool CreateDirectory(string directoryPath, bool isRecurseCreate = false)
        {
            return FtpUtils.CreateDirectory(UriWithRoot(directoryPath), Credentials, isRecurseCreate);
        }

        public bool DownloadFileFromServer(string path, FileInfo localFile)
        {
            return FtpUtils.DownloadFileFromServer(UriWithRoot(path), Credentials, localFile);
        }

        public bool RenameFile(string path, string newNameFile)
        {
            return FtpUtils.RenameFile(UriWithRoot(path), newNameFile, Credentials);
        }

        public bool UploadFileToServer(string directoryPath, FileInfo localFile)
        {
            return FtpUtils.UploadFileToServer(UriWithRoot(directoryPath), Credentials, localFile);
        }

        public bool IsFileExists(string path)
        {
            return FtpUtils.IsFileExists(UriWithRoot(path), Credentials);
        }

        public bool IsDirectoryExists(string directoryPath)
        {
            return FtpUtils.IsDirectoryExists(UriWithRoot(directoryPath), Credentials);
        }

        public bool IsOkToConnect()
        {
            return FtpUtils.IsOkToConnect(RootUri, Credentials);
        }

        public Stream GetUploadStream(string remoteFilePath)
        {
            Uri remoteUri = UriWithRoot(remoteFilePath);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteUri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = Credentials;
            request.KeepAlive = true;
            request.UsePassive = true;
            request.UseBinary = true;
        }


        private Uri UriWithRoot(string relativePath)
        {
            String partRoot = RootUri.AbsoluteUri;
            partRoot = partRoot.EndsWith("/") ? partRoot.Substring(0, partRoot.Length-1) : partRoot ;
            relativePath = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
            return new Uri(partRoot + relativePath);

        }
    }
}
