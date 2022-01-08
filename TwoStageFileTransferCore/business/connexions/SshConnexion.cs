using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Renci.SshNet;
using Renci.SshNet.Common;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferCore.business.connexions
{

    public class SshConnexion : IConnexion
    {
        private static readonly Logger _log = Logger.LastLoggerInstance;

        public NetworkCredential Credentials { get; set; }
        public string Host { get; private set; }
        public int Port { get; private set; }

        private readonly SftpClient _innerSftpClient;

        public SshConnexion(NetworkCredential credential, string host, int port)
        {
            Credentials = credential;
            Host = host;
            Port = port;

            var kiconnectInfo = new KeyboardInteractiveAuthenticationMethod(credential.UserName);
            kiconnectInfo.AuthenticationPrompt += delegate (object sender, AuthenticationPromptEventArgs e)
            {
                foreach (var prompt in e.Prompts)
                {
                    if (prompt.Request.ToLowerInvariant().StartsWith("password"))
                    {
                        prompt.Response = credential.Password;
                    }
                }
            };

            List<AuthenticationMethod> authMethods = new List<AuthenticationMethod>
            {
                kiconnectInfo,
                new PasswordAuthenticationMethod(credential.UserName, credential.Password)
            };


            ConnectionInfo sshConnectionInfo = new ConnectionInfo(host, port, credential.UserName, authMethods.ToArray());
            sshConnectionInfo.Timeout = TimeSpan.FromSeconds(10);

            _innerSftpClient = new SftpClient(sshConnectionInfo);
            IsOkToConnect();
        }

        public bool DeleteFile(string path)
        {
            CheckSftpClientConnected();

            try
            {
                _innerSftpClient.DeleteFile(path);
                return true;
            }
            catch (Exception ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }
        }

        public bool DeleteFile(Uri path)
        {
            return DeleteFile(path.AbsolutePath);
        }

        public bool CreateDirectory(string directoryPath, bool isRecurseCreate = false)
        {
            CheckSftpClientConnected();

            try
            {
                _innerSftpClient.CreateDirectory(directoryPath);
                return true;
            }
            catch (SftpPermissionDeniedException ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }

        }

        public bool CreateDirectory(Uri directoryPath, bool isRecurseCreate = false)
        {
            return CreateDirectory(directoryPath.AbsolutePath, isRecurseCreate);
        }

        public bool DownloadFileFromServer(string path, FileInfo localFile)
        {
            CheckSftpClientConnected();

            try
            {
                using (FileStream writer = File.Create(localFile.FullName))
                {
                    _innerSftpClient.DownloadFile(path, writer);
                }

                return _innerSftpClient.Get(path).Length == localFile.Length;

            }
            catch (Exception ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }
        }

        public bool DownloadFileFromServer(Uri path, FileInfo localFile)
        {
            return DownloadFileFromServer(path.AbsolutePath, localFile);
        }

        public bool RenameFile(string path, string newNameFile)
        {
            CheckSftpClientConnected();

            try
            {
                string[] pathSegments = path.Split('/');
                string newNamePath = string.Join("/", pathSegments.Take(pathSegments.Length - 1)) + $"/{newNameFile}";

                _innerSftpClient.RenameFile(path, newNamePath);

                return true;
            }
            catch (Exception ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }
        }

        public bool RenameFile(Uri path, string newNameFile)
        {
            return RenameFile(path.AbsolutePath, newNameFile);
        }

        public bool UploadFileToServer(string directoryPath, FileInfo localFile)
        {
            CheckSftpClientConnected();

            try
            {
                string remoteFilePath = $"{directoryPath}/{localFile.Name}";
                using (FileStream input = File.OpenRead(localFile.FullName))
                {
                    _innerSftpClient.UploadFile(input, remoteFilePath);
                }

                return _innerSftpClient.Get(remoteFilePath).Length == localFile.Length;

            }
            catch (Exception ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }
        }

        public bool UploadFileToServer(Uri directoryPath, FileInfo localFile)
        {
            return UploadFileToServer(directoryPath.AbsolutePath, localFile);
        }

        public bool IsFileExists(string path)
        {
            CheckSftpClientConnected();
            try
            {
                return _innerSftpClient.Exists(path) && _innerSftpClient.GetAttributes(path).IsRegularFile;
            }
            catch (SftpPermissionDeniedException ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }
        }

        public bool IsFileExists(Uri path)
        {
            return IsFileExists(path.AbsolutePath);
        }

        public bool IsDirectoryExists(string directoryPath)
        {
            CheckSftpClientConnected();
            try
            {
                return _innerSftpClient.Exists(directoryPath) && _innerSftpClient.GetAttributes(directoryPath).IsDirectory;

            }
            catch (SftpPermissionDeniedException ex)
            {
                LogUtils.D(_log, ex.Message);
                LogUtils.D(_log, ex.StackTrace);
                return false;
            }
        }



        public bool IsDirectoryExists(Uri directoryPath)
        {
            return IsDirectoryExists(directoryPath.AbsolutePath);
        }

        public bool IsOkToConnect()
        {
            if (_innerSftpClient == null) throw new Exception("Connexion not initialized correctly.");

            if (_innerSftpClient != null && _innerSftpClient.IsConnected) return true;

            try
            {

                _innerSftpClient.Connect();
                return _innerSftpClient.IsConnected;
            }
            catch (SshConnectionException ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "Error while connecting using SFTP");
                return false;
            }
            catch (SshAuthenticationException ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "Error while connecting using SFTP");
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void DownloadFromServer(Stream fileDownloadedStream, string fileAbsolutePath, Action<ulong> uploadCallback = null)
        {
            CheckSftpClientConnected();

            DateTime dtT = DateTime.Now.AddSeconds(10);
            DateTime dtRef = DateTime.Now;

            void CallBackAction(ulong obj)
            {
                uploadCallback?.Invoke(obj);
                if (DateTime.Now <= dtT) return;

                _log.Debug($"Transfered {obj}");
                dtT = dtT.AddSeconds(10);
                _log.Debug($" >> {dtT}");
            }

            object objState = "null";
            var upload = _innerSftpClient.BeginDownloadFile(fileAbsolutePath, fileDownloadedStream, null, objState, CallBackAction);


            while (!upload.IsCompleted && dtRef < dtT)
            {
                upload.AsyncWaitHandle.WaitOne(1000);
                dtRef = DateTime.Now;

            }

            if (!upload.IsCompleted)
            {
                throw new Exception("Download file with SSH hangs more than 10 sec");
            }
            else
            {
                _innerSftpClient.EndDownloadFile(upload);
            }
        }


        public void UploadStreamToServer(Stream stream, string fileAbsolutePath, bool canOverride = false, Action<ulong> uploadCallback = null)
        {
            /*
             * To upload a file to the server, the implementation below is used instead of the synchronous method ScpClient::UploadFile(...).
             * The synchronous method works very well, but has a flaw: if the server is full, the method never stops, even after the connection
             * timeout.
             *
             * So here the upload is started aynchronously.Then, in a loop, we wait for 1 second that the sending works or that the timeout
             * (of 10 seconds) has expired. At each turn of the loop, we redefine the current time. In the upload progress callback (which is
             * called when the upload is actually working, but not when it freezes), we increase the target time of the timeout. If the upload
             * freezes, then the callback is not played, and the condition of the loop becomes false. Finally, we check if the upload is
             * complete: if not, then we send an exception.
             *
             * Translated with www.DeepL.com/Translator (free version)
             */

            CheckSftpClientConnected();

            DateTime dtT = DateTime.Now.AddSeconds(10);
            DateTime dtRef = DateTime.Now;

            void CallBackAction(ulong obj)
            {

                uploadCallback?.Invoke(obj);
                if (DateTime.Now <= dtT) return;

                _log.Debug($"Transfered {obj}");
                dtT = dtT.AddSeconds(10);
                _log.Debug($" >> {dtT}");
            }

            object objState = "null";
            var upload = _innerSftpClient.BeginUploadFile(stream, fileAbsolutePath, canOverride, null, objState, (Action<ulong>)CallBackAction);


            while (!upload.IsCompleted && dtRef < dtT)
            {
                upload.AsyncWaitHandle.WaitOne(1000);
                dtRef = DateTime.Now;

            }

            if (!upload.IsCompleted)
            {
                //_innerSftpClient.EndUploadFile(upload);
                throw new Exception("Upload file with SSH hangs more than 10 sec");
            }
            else
            {
                _innerSftpClient.EndUploadFile(upload);
            }


        }

        public void Close()
        {
            if (_innerSftpClient != null && _innerSftpClient.IsConnected)
            {
                _innerSftpClient.Disconnect();
            }
            _innerSftpClient?.Dispose();
        }


        /// <summary>
        /// Checks that the client has been initialized, and that the connection is active.
        /// </summary>
        private void CheckSftpClientConnected()
        {
            if (_innerSftpClient == null || !_innerSftpClient.IsConnected)
                throw new Exception("Connexion not initialized correctly or lost.");
        }
    }
}
