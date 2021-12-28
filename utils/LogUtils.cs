using AryxDevLibrary.utils.logger;
using System;

namespace TwoStageFileTransfer.utils
{
    static class LogUtils
    {

        internal static void I(Logger log, string what, bool isWriteConsole=true, bool isWriteLog=true)
        {
            if (isWriteConsole)
            {
                Console.WriteLine(what);
            }

            if (isWriteLog)
            {
                log.Info(what);
            }
        }

        internal static void D(Logger log, string what, bool isWriteConsole = true, bool isWriteLog = true)
        {
            if (isWriteConsole)
            {
                Console.WriteLine(what);
            }

            if (isWriteLog)
            {
                log.Debug(what);
            }
        }

        internal static void E(Logger log, string what, bool isWriteConsole = true, bool isWriteLog = true)
        {
            if (isWriteConsole)
            {
                Console.WriteLine(what);
            }

            if (isWriteLog)
            {
                log.Error(what);
            }
        }


        internal static void W(Logger log, string what, bool isWriteConsole = true, bool isWriteLog = true)
        {
            if (isWriteConsole)
            {
                Console.WriteLine(what);
            }

            if (isWriteLog)
            {
                log.Warn(what);
            }
        }
    }
}
