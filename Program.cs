using AryxDevLibrary.utils.logger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.cliParser;
using TwoStageFileTransfer.business;
using TwoStageFileTransfer.business.moderuns;
using TwoStageFileTransfer.business.transferworkers.@in;
using TwoStageFileTransfer.business.transferworkers.@out;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using TwoStageFileTransfer.constant;
using static TwoStageFileTransfer.utils.LogUtils;
using FileUtils = TwoStageFileTransfer.utils.FileUtils;
using ProcessUtils = TwoStageFileTransfer.utils.ProcessUtils;

namespace TwoStageFileTransfer
{
    class Program
    {
        private static Logger _log = null;

        private static readonly string AppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwoStageFileTransfer");

        private static AppArgs _appArgs;

        private static AbstractModeRun _modeRun;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (!Directory.Exists(AppDataDir))
                {
                    Directory.CreateDirectory(AppDataDir);
                }

                _log = new Logger(Path.Combine(AppDataDir, "log.log"), Logger.LogLvl.NONE, Logger.LogLvl.DEBUG, "1 Mo");

                AppHeader();

                AppArgsParser argsParser = new AppArgsParser();
                _appArgs = GetAppArgs(args, argsParser);


                InitLogAndAskForParams(_appArgs);

                I(_log, $"Source: {_appArgs.Source}");
                I(_log, $"Target: {_appArgs.Target}");

                Console.WriteLine();
                DoWork(_appArgs, argsParser);
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "MainError");
                AppUtils.Exit(EnumExitCodes.KO);
            }

            AppUtils.Exit(EnumExitCodes.OK);
        }

        private static AppArgs GetAppArgs(String[] args, AppArgsParser argsParser)
        {
            string sourceProcessName = ProcessUtils.ParentProcessUtilities.GetParentProcess().ProcessName.ToLower();

            AppArgs appArgs = null;

            try
            {
                if (args.Length == 1 && args[0].ToLower().EndsWith(".tsft"))
                {
                    appArgs = new AppArgs() { Direction = AppCst.MODE_OUT, Source = args[0] };
                    D(_log, "Run with tsft file : Mode OUT");
                }
                else
                {
                    appArgs = argsParser.ParseDirect(args);
                    D(_log, "Run with parameters");
                    I(_log, $"Args input: {argsParser.StrCommand}");

                }
            }
            catch (CliParsingException e)
            {
                if (args.Any())
                {
                    I(_log, e.Message);
                }

                argsParser.ShowSyntax();
                if (appArgs == null && "explorer".Equals(sourceProcessName))
                {
                    Console.Read();
                }

                AppUtils.Exit(EnumExitCodes.KO_PARAMS_PARSING);
                return null;
            }

            appArgs.SourceRun = SourceRuns.Undetermined;
            switch (sourceProcessName)
            {

                case "explorer":
                    appArgs.SourceRun = SourceRuns.Explorer;
                    _modeRun = new ExplorerRun();
                    break;
                case "cmd":
                    appArgs.SourceRun = SourceRuns.CommandPrompt;
                    _modeRun = new CmdRun();
                    break;
                default:
                    appArgs.SourceRun = SourceRuns.CommandPrompt;
                    _modeRun = new CmdRun();
                    break;
            }

            return appArgs;

        }

        private static void InitLogAndAskForParams(AppArgs appArgs)
        {
            _modeRun.InitLogAndAskForParams(appArgs);
            switch (appArgs.Direction)
            {
                case AppCst.MODE_IN:
                    _log = new Logger(Path.Combine(AppDataDir, "log-IN.log"), Logger.LogLvl.NONE, Logger.LogLvl.DEBUG, "1 Mo");
                    break;
                case AppCst.MODE_OUT:
                    _log = new Logger(Path.Combine(AppDataDir, "log-OUT.log"), Logger.LogLvl.NONE, Logger.LogLvl.INFO, "1 Mo");
                    break;
            }
        }

        private static void DoWork(AppArgs appArgs, AppArgsParser argsParser)
        {
            try
            {
                switch (appArgs.Direction)
                {
                    case AppCst.MODE_IN:
                        {
                            I(_log, "First stage: transfer file from source to temp-shared folder");

                            long maxTransferLenght = (long)(FileUtils.GetAvailableSpace(appArgs.Target, 20 * 1024 * 1024) * 0.9);
                            I(_log, $"Max size that can be used: {AryxDevLibrary.utils.FileUtils.HumanReadableSize(maxTransferLenght)}");

                            InWorkOptions jobOptions = new InWorkOptions()
                            {
                                MaxSizeUsedOnShared = maxTransferLenght,
                                PartFileSize = appArgs.ChunkSize,
                                Source = new FileInfo(appArgs.Source),
                                Target = appArgs.Target,
                                BufferSize = appArgs.BufferSize,
                                CanOverwrite = appArgs.CanOverwrite
                            };

                            InToOutWork w = new InToOutWork(jobOptions);

                            w.DoTransfert();
                            break;
                        }
                    case AppCst.MODE_OUT:
                        {
                            I(_log, "Second stage: recompose target file from part files from shared folder");

                            OutWorkOptions jobOptions = new OutWorkOptions()
                            {
                                Source = new FileInfo(appArgs.Source),
                                Target = appArgs.Target,
                                BufferSize = appArgs.BufferSize,
                                CanOverwrite = appArgs.CanOverwrite
                            };

                            OutToFileWork o = new OutToFileWork(jobOptions);

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
                E(_log, "", isWriteLog: false);
                E(_log, "Error: " + e.Message);
                E(_log, e.StackTrace, false);

                if (appArgs.IsRunByExplorer())
                {
                    Console.Read();
                }

            }
            finally
            {
                Console.WriteLine();
            }
        }

        private static void AppHeader()
        {
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------------------------");
# if DEBUG
            Console.WriteLine("   TWO-STAGE FILE TRANSFER  ::  {0} - DEBUG", Assembly.GetExecutingAssembly().GetName().Version);
#else
            Console.WriteLine("   TWO-STAGE FILE TRANSFER  ::  {0}", Assembly.GetExecutingAssembly().GetName().Version);
#endif
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine();
        }



    }
}
