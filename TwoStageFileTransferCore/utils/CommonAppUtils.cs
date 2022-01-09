using System;

namespace TwoStageFileTransferCore.utils
{
    public static class CommonAppUtils
    {
        public static Uri ToUri(this string str)
        {
            return new Uri(str);
        }

        public static string GetTransferSpeed(long localBytesRead, DateTime timeStart)
        {
            long diffTime = (long)(DateTime.Now - timeStart).TotalSeconds;
            if (diffTime == 0) return string.Empty;

            return "~" + AryxDevLibrary.utils.FileUtils.HumanReadableSize(localBytesRead / diffTime) + "/s [last part]";
        }
    }
}
