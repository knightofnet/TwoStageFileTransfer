using System;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransfer.constant;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransfer.utils
{
    public static class AppUtils
    {
        private static readonly Logger _log = Logger.LastLoggerInstance;

        internal static void Exit(EnumExitCodes exitCodes)
        {
            LogUtils.I(_log, $"End: {exitCodes.Index} -> {exitCodes.Libelle}");
            Environment.Exit(exitCodes.Index);
        }

        public static string GetTransferSpeed(long localBytesRead, DateTime timeStart)
        {
            long diffTime = (long)(DateTime.Now - timeStart).TotalSeconds;
            if (diffTime == 0) return string.Empty;

            return "~" + AryxDevLibrary.utils.FileUtils.HumanReadableSize(localBytesRead / diffTime) + "/s [last part]";
        }

        public static Uri ToUri(this string str)
        {
            return new Uri(str);
        }
    }
}
