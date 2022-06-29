using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransferCore.constant;

namespace TwoStageFileTransferCore.utils
{
    
    public static class FileUtils
    {
        private static readonly Logger _log = Logger.LastLoggerInstance;

        public static String GetFileName(string origNameFile, long fileSize, long part)
        {
            return origNameFile + "." + fileSize + ".part" + part;
        }

        public static long GetAvailableSpace(string target, long defaultRet)
        {
            DirectoryInfo t = new DirectoryInfo(target);

            /*
            if (!t.Exists)
            {
                throw new Exception(target + " doesnt exist");
            }
            */

            if (target.StartsWith(@"\\"))
            {
                long networkSize = GetFreeSpaceNetworkShare(target);
                return networkSize == -1 ? defaultRet : networkSize;
            }

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (!drive.IsReady)
                {
                    _log.Debug("{0} is not ready", drive.Name);
                    continue;
                }

                if (drive.RootDirectory.FullName.Equals(t.Root.FullName.ToUpper()))
                {

                    return drive.AvailableFreeSpace;
                }

                _log.Debug("{0} != {1} ", drive.RootDirectory.FullName, t.Root.FullName);
            }

            return defaultRet;

        }

        public static string GetSha1Hash(FileInfo file)
        {
            using (FileStream stream = File.OpenRead(file.FullName))
            {
                using (SHA1Managed sha = new SHA1Managed())
                {
                    byte[] checksum = sha.ComputeHash(stream);
                    string sendCheckSum = BitConverter.ToString(checksum)
                        .Replace("-", string.Empty);

                    return sendCheckSum;
                }
            }


        }

        /// <summary>
        /// https://social.msdn.microsoft.com/Forums/en-US/b7db7ec7-34a5-4ca6-89e7-947190c4e043/get-free-space-on-network-share?forum=csharpgeneral
        /// </summary>
        /// <param name="networkPath"></param>
        /// <returns></returns>
        public static long GetFreeSpaceNetworkShare(string networkPath)
        {
            if (!networkPath.EndsWith(@"\"))
            {
                networkPath += @"\";
            }
            long free = 0, dummy1 = 0, dummy2 = 0;
            if (GetDiskFreeSpaceEx(networkPath, ref free, ref dummy1, ref dummy2))
            {
                return free;
            }

            return -1;
        }

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx
        (
            string lpszPath,                    // Must name a folder, must end with '\'.
            ref long lpFreeBytesAvailable,
            ref long lpTotalNumberOfBytes,
            ref long lpTotalNumberOfFreeBytes
        );

        /// <summary>
        /// Ask for a filepath, test what user type, and if not a valid file, ask again.
        /// </summary>
        /// <param name="promptPhrase"></param>
        /// <returns>valid filepath</returns>
        public static string ConsoleGetValidFilepath(string promptPhrase = "Enter filepath: ")
        {
            Console.WriteLine(promptPhrase);
            string rawSource = Console.ReadLine()?.Trim(AppCst.TrimPathChars);
            while (!File.Exists(rawSource))
            {
                Console.WriteLine("The file '{0}' doesnt exist.", rawSource);
                rawSource = Console.ReadLine()?.Trim(AppCst.TrimPathChars);
            }

            return rawSource;
        }

        /// <summary>
        /// Ask for a directory-path, test what user type, and if not a valid directory, ask again.
        /// </summary>
        /// <param name="promptPhrase"></param>
        /// <returns>valid directory-path</returns>
        public static string ConsoleGetValidDirectorypath(string promptPhrase = "Enter directory-path: ",
            bool createIfNotExist = false)
        {
            Console.WriteLine(promptPhrase);
            string rawSource = Console.ReadLine()?.Trim(AppCst.TrimPathChars);
            if (!Directory.Exists(rawSource) && createIfNotExist)
            {
                Directory.CreateDirectory(rawSource);
                return rawSource;
            }

            while (!Directory.Exists(rawSource))
            {
                Console.WriteLine("The directory '{0}' doesnt exist.", rawSource);
                rawSource = Console.ReadLine()?.Trim(AppCst.TrimPathChars);
            }

            return rawSource;
        }

        
        public static string WinformGetValidFilepath(string promptPhrase = "Select filepath ")
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = promptPhrase;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    return openFileDialog.FileName;
                }
            }

            return null;
        }

        public static string WinformGetValidDirectorypath(string promptPhrase = "Select directory ")
        {
            FolderPicker dlg = new FolderPicker();
            dlg.Title = promptPhrase;
            dlg.InputPath = "c:\\";
            if (dlg.ShowDialog(IntPtr.Zero) == true)
            {
                return dlg.ResultPath;
            }

            return null;
        }

        public static string WinformGetValidSaveFilepath(string promptPhrase = "Select filepath ")
        {
            using (SaveFileDialog openFileDialog = new SaveFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = promptPhrase;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    return openFileDialog.FileName;
                }
            }

            return null;
        }

        public static string CalculculateSourceSha1(FileInfo file, string compSha1 = null)
        {
            Console.Write((string)"Calculate SHA1... ", (object)_log);
            _log.Info("Calculate SHA1");
            DateTime start = DateTime.Now;

            string sha1 = GetSha1Hash(file);
            Console.WriteLine("Done.");


            if (compSha1 != null)
            {
                LogUtils.I(_log, $"SHA1 match: {(sha1.ToUpper().Equals(compSha1.ToUpper()) ? "OK" : "KO")}");
            }

            TimeSpan duration = DateTime.Now - start;
            _log.Debug("Calculate Sha1 > Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));

            return sha1;
        }
    }
}
