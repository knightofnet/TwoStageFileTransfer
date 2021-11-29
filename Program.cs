using AryxDevLibrary.utils.logger;
using System;
using System.IO;
using TwoStageFileTransfer.business;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer
{
    class Program
    {
        private static Logger _log = null;

        static void Main(string[] args)
        {

            AppArgsParser argsParser = new AppArgsParser();
            AppArgs appArgs = argsParser.ParseDirect(args);
            if (appArgs.Direction == "IN")
            {
                _log = new Logger("log-IN.log", Logger.LogLvl.NONE, Logger.LogLvl.INFO, "1 Mo");
            } else if (appArgs.Direction == "OUT")
            {
                _log = new Logger("log-OUT.log", Logger.LogLvl.NONE, Logger.LogLvl.INFO, "1 Mo");
            }

            // args = new string[3] {"in", @"C:\Users\ARyx\Desktop\Badger2018-08-06am.7z", @"C:\Users\ARyx\Desktop\destination"};
            // args = new string[3] {"out", @"C:\Users\ARyx\Desktop\destination\Badger2018-08-06am.7z.29445130.part0", @"C:\Users\ARyx\Desktop\destination\final"};
            // args = new string[3] { "out", @"C:\Users\ARyx\Desktop\destination", @"C:\Users\ARyx\Desktop\destination\final" };

            LogUtils.WriteConsole(String.Format("Source: {0}", appArgs.Source), _log);
            LogUtils.WriteConsole(String.Format("Target: {0}", appArgs.Target), _log);
            Console.WriteLine();

            if (appArgs.Direction == "IN")
            {
                LogUtils.WriteConsole("Mode IN", _log);

                InToOutWork w = new InToOutWork();
                w.Source = new FileInfo(appArgs.Source);
                w.Target = appArgs.Target;
                w.BufferSize = appArgs.BufferSize;

                w.MaxTransfertLength = (long) (FileUtils.GetAvailableSpace(w.Target, 20 * 1024 * 1024) * 0.9);
                LogUtils.WriteConsole(String.Format("Max size used: {0}", AryxDevLibrary.utils.FileUtils.HumanReadableSize(w.MaxTransfertLength)), _log);

                w.DoTransfert();

            } else if (appArgs.Direction == "OUT")
            {

                LogUtils.WriteConsole("Mode OUT", _log);

                OutToFileWork o = new OutToFileWork();
                o.Source = new FileInfo(appArgs.Source);
                o.Target = appArgs.Target;
                o.BufferSize = appArgs.BufferSize;

                o.DoTransfert();

            }


        }
    }
}
