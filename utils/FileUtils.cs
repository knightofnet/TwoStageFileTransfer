using AryxDevLibrary.utils.logger;
using System;
using System.IO;
using System.Security.Cryptography;

namespace TwoStageFileTransfer.utils
{
    static class FileUtils
    {
        private static Logger _log = Logger.LastLoggerInstance;

        public static String GetFileName(string origNameFile, long fileSize, int part)
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
            foreach(DriveInfo drive in drives)
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
    }
}
