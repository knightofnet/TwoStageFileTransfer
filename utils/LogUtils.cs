using AryxDevLibrary.utils.logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransfer.utils
{
    static class LogUtils
    {
        internal static void WriteConsole(string what, Logger log)
        {
            Console.WriteLine(what);
            log.Info(what);
        }
    }
}
