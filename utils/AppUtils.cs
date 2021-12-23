using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransfer.constant;

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
    }
}
