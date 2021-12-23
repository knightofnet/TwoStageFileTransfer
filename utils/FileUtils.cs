using AryxDevLibrary.utils.logger;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using TwoStageFileTransfer.constant;

namespace TwoStageFileTransfer.utils
{
    
    static class FileUtils
    {
        private static readonly Logger _log = Logger.LastLoggerInstance;

        public static String GetFileName(string origNameFile, long fileSize, long part)
        {
            return origNameFile + "." + fileSize + ".part" + part;
        }

        internal static long GetAvailableSpace(string target, long defaultRet)
        {
            DirectoryInfo t = new DirectoryInfo(target);

            if (!t.Exists)
            {
                throw new Exception(target + " doesnt exist");
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
        /// Ask for a filepath, test what user type, and if not a valid file, ask again.
        /// </summary>
        /// <param name="promptPhrase"></param>
        /// <returns>valid filepath</returns>
        public static string ConsoleGetValidFilepath(string promptPhrase = "Enter filepath: ")
        {
            Console.WriteLine(promptPhrase);
            string rawSource = Console.ReadLine()?.Trim(AppCst.TRIM_PATH_CHARS);
            while (!File.Exists(rawSource))
            {
                Console.WriteLine("The file '{0}' doesnt exist.", rawSource);
                rawSource = Console.ReadLine()?.Trim(AppCst.TRIM_PATH_CHARS);
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
            string rawSource = Console.ReadLine()?.Trim(AppCst.TRIM_PATH_CHARS);
            if (!Directory.Exists(rawSource) && createIfNotExist)
            {
                Directory.CreateDirectory(rawSource);
                return rawSource;
            }

            while (!Directory.Exists(rawSource))
            {
                Console.WriteLine("The directory '{0}' doesnt exist.", rawSource);
                rawSource = Console.ReadLine()?.Trim(AppCst.TRIM_PATH_CHARS);
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
    }
}
