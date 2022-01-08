using System;
using AryxDevLibrary.utils.logger;

namespace TwoStageFileTransferCore.utils
{
    public static class LogUtils
    {

        public static void I(Logger log, string what, bool isWriteConsole=true, bool isWriteLog=true)
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

        public static void D(Logger log, string what, bool isWriteConsole = true, bool isWriteLog = true)
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

        public static void E(Logger log, string what, bool isWriteConsole = true, bool isWriteLog = true)
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


        public static void W(Logger log, string what, bool isWriteConsole = true, bool isWriteLog = true)
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
