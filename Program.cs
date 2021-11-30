using AryxDevLibrary.utils.logger;
using System;
using System.IO;
using AryxDevLibrary.utils.cliParser;
using TwoStageFileTransfer.business;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using TwoStageFileTransfer.constant;

namespace TwoStageFileTransfer
{
    class Program
    {
        private static Logger _log = _log = new Logger("log.log", Logger.LogLvl.NONE, Logger.LogLvl.INFO, "1 Mo");

        static void Main(string[] args)
        {
            AppArgs appArgs;
            AppArgsParser argsParser = new AppArgsParser();
            try
            {
                appArgs = argsParser.ParseDirect(args);
            }
            catch (CliParsingException e)
            {
                LogUtils.WriteConsole(e.Message, _log);
                argsParser.ShowSyntax();

                Environment.Exit(1);
                return;
            }

            InitLogAndAskForParams(appArgs);

            LogUtils.WriteConsole(string.Format("Source: {0}", appArgs.Source), _log);
            LogUtils.WriteConsole(string.Format("Target: {0}", appArgs.Target), _log);
            Console.WriteLine();
            DoWork(appArgs, argsParser);

        }

        private static void DoWork(AppArgs appArgs, AppArgsParser argsParser)
        {
            try
            {

                switch (appArgs.Direction)
                {
                    case AppCst.MODE_IN:
                        {
                            LogUtils.WriteConsole("Mode IN", _log);

                            long maxTransferLenght = (long)(FileUtils.GetAvailableSpace(appArgs.Target, 20 * 1024 * 1024) * 0.9);
                            LogUtils.WriteConsole(string.Format("Max size that can be used: {0}", AryxDevLibrary.utils.FileUtils.HumanReadableSize(maxTransferLenght)), _log);

                            InToOutWork w = new InToOutWork(maxTransferLenght, appArgs.ChunkSize, appArgs.IsDoCompress);
                            w.Source = new FileInfo(appArgs.Source);
                            w.Target = appArgs.Target;
                            w.BufferSize = appArgs.BufferSize;

                            w.DoTransfert();
                            break;
                        }
                    case AppCst.MODE_OUT:
                        {
                            LogUtils.WriteConsole("Mode OUT", _log);

                            OutToFileWork o = new OutToFileWork();
                            o.Source = new FileInfo(appArgs.Source);
                            o.Target = appArgs.Target;
                            o.BufferSize = appArgs.BufferSize;

                            o.DoTransfert();
                            break;
                        }
                    default:
                        argsParser.ShowSyntax();
                        break;
                }
            }
            catch (Exception e)
            {
                LogUtils.WriteConsole("Error : " + e.Message, _log);
                _log.Error(e.StackTrace);

            }
            finally
            {
                Console.WriteLine();
            }
        }

        private static void InitLogAndAskForParams(AppArgs appArgs)
        {
            if (appArgs.Direction == AppCst.MODE_IN)
            {
                _log = new Logger("log-IN.log", Logger.LogLvl.NONE, Logger.LogLvl.INFO, "1 Mo");

                if (appArgs.Source == null)
                {
                    Console.WriteLine("Enter source file to transfer: ");
                    string rawSource = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    while (!File.Exists(rawSource))
                    {
                        Console.WriteLine("The file '{0}' doesnt exist.", rawSource);
                        rawSource = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    }
                    appArgs.Source = rawSource;
                }

                if (appArgs.Target == null)
                {
                    Console.WriteLine("Enter target file to transfer the part files: ");
                    string rawTarget = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    while (!Directory.Exists(rawTarget))
                    {
                        Console.WriteLine("The target '{0}' doesnt exist.", rawTarget);
                        rawTarget = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    }
                    appArgs.Target = rawTarget;
                }
            }
            else if (appArgs.Direction == AppCst.MODE_OUT)
            {
                _log = new Logger("log-OUT.log", Logger.LogLvl.NONE, Logger.LogLvl.INFO, "1 Mo");

                if (appArgs.Source == null)
                {
                    Console.WriteLine("Enter source folder to transfer for, or path to tstr file: ");
                    string rawSource = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    while (!File.Exists(rawSource) && !Directory.Exists(rawSource))
                    {
                        Console.WriteLine("The source '{0}' doesnt exist.", rawSource);
                        rawSource = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    }
                }

                if (appArgs.Target == null)
                {
                    Console.WriteLine("Enter target file to recompose part file: ");
                    string rawTarget = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    while (!File.Exists(rawTarget))
                    {
                        Console.WriteLine("The target '{0}' doesnt exist.", rawTarget);
                        rawTarget = Console.ReadLine()?.Trim(new[] { ' ', '"' });
                    }
                }
            }

        }
    }
}
