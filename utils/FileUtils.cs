using System;
using System.IO;

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

                //Console.WriteLine("T:{0}, d:{1}", t.Root.FullName, drive.RootDirectory.FullName);

                if (drive.RootDirectory.FullName.Equals(t.Root.FullName))
                {
                    //Console.WriteLine("T:{0}, d:{1}", t.Root.FullName, drive.RootDirectory.FullName);
                    return drive.AvailableFreeSpace;
                }
                
            }

            return defaultRet;

        }
    }
}
