using System;
using System.IO;
using System.Security.Cryptography;

namespace TwoStageFileTransfer.utils
{
    static class FileUtils
    {

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
                    continue;
                }

                if (drive.RootDirectory.FullName.Equals(t.Root.FullName))
                {
                    
                    return drive.AvailableFreeSpace;
                }                
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
