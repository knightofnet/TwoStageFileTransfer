using System;

namespace TwoStageFileTransfer.utils
{
    static class FileUtils
    {

        public static String GetFileName(string origNameFile, long fileSize, int part)
        {
            return origNameFile + "." + fileSize + ".part" + part;
        }

    }
}
