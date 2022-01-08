using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils;
using Renci.SshNet.Messages.Connection;

namespace TwoStageFileTransfer.business.connexions
{
    public class FtpConnexion : IConnexion
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

        public bool DeleteFile(Uri path)
        {
            return FtpUtils.DeleteFile(path, Credentials);
        }

        public bool CreateDirectory(string directoryPath, bool isRecurseCreate = false)
        {
            return FtpUtils.CreateDirectory(UriWithRoot(directoryPath), Credentials, isRecurseCreate);
        }

        public bool CreateDirectory(Uri directoryPath, bool isRecurseCreate = false)
        {
            return FtpUtils.CreateDirectory(directoryPath, Credentials, isRecurseCreate);
        }

        public bool DownloadFileFromServer(string path, FileInfo localFile)
        {
            return FtpUtils.DownloadFileFromServer(UriWithRoot(path), Credentials, localFile);
        }

        public bool DownloadFileFromServer(Uri path, FileInfo localFile)
        {
            return FtpUtils.DownloadFileFromServer(path, Credentials, localFile);
        }

        public bool RenameFile(string path, string newNameFile)
        {
            return FtpUtils.RenameFile(UriWithRoot(path), newNameFile, Credentials);
        }

        public bool RenameFile(Uri path, string newNameFile)
        {
            return FtpUtils.RenameFile(path, newNameFile, Credentials);
        }

        public bool UploadFileToServer(string directoryPath, FileInfo localFile)
        {
            return FtpUtils.UploadFileToServer(UriWithRoot(directoryPath), Credentials, localFile);
        }

        public bool UploadFileToServer(Uri directoryPath, FileInfo localFile)
        {
            return FtpUtils.UploadFileToServer(directoryPath, Credentials, localFile);
        }

        public bool IsFileExists(string path)
        {
            return FtpUtils.IsFileExists(UriWithRoot(path), Credentials);
        }

        public bool IsFileExists(Uri path)
        {
            return FtpUtils.IsFileExists(path, Credentials);
        }

        public bool IsDirectoryExists(string directoryPath)
        {
            return FtpUtils.IsDirectoryExists(UriWithRoot(directoryPath), Credentials);
        }

        public bool IsDirectoryExists(Uri directoryPath)
        {
            return FtpUtils.IsDirectoryExists(directoryPath, Credentials);
        }

        public bool IsOkToConnect()
        {
            return FtpUtils.IsOkToConnect(RootUri, Credentials);
        }

        public void UploadStreamToServer(Stream stream, string fileAbsolutePath, bool canOverride = false, Action<ulong> uploadCallback = null)
        {
            
        }

        public void Close()
        {
            
        }


        private Uri UriWithRoot(string relativePath)
        {
            //return UriUtils.NewFtpUri(relativePath);

            String partRoot = RootUri.AbsoluteUri;
            partRoot = partRoot.EndsWith("/") ? partRoot.Substring(0, partRoot.Length-1) : partRoot ;
            relativePath = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
            return new Uri(partRoot + relativePath);

        }
    }
}
