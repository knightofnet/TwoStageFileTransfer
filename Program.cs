using AryxDevLibrary.utils.logger;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.cliParser;
using TwoStageFileTransfer.business;
using TwoStageFileTransfer.business.connexions;
using TwoStageFileTransfer.business.moderuns;
using TwoStageFileTransfer.business.transferworkers;
using TwoStageFileTransfer.business.transferworkers.inwork;
using TwoStageFileTransfer.business.transferworkers.outwork;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.exceptions;
using static TwoStageFileTransfer.utils.LogUtils;
using FileUtils = TwoStageFileTransfer.utils.FileUtils;
using ProcessUtils = TwoStageFileTransfer.utils.ProcessUtils;

namespace TwoStageFileTransfer
{
    class Program
    {
        private static Logger _log;

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
                ExceptionHandlingUtils.Logger = _log;

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
                bool isTsftFile = false;
                string tsftFilePath = null;
                if (args.Length == 1 && args[0].ToLower().EndsWith(".tsft"))
                {
                    appArgs = new AppArgs() { Direction = DirectionTrts.OUT, Source = args[0], TransferType = TransferTypes.Windows };
                    isTsftFile = true;
                    tsftFilePath = args[0];
                    D(_log, "Run with tsft file : Mode OUT");

                }
                else
                {
                    appArgs = argsParser.ParseDirect(args);
                    D(_log, "Run with parameters");
                    I(_log, $"Args input: {argsParser.StrCommand}");

                    if (appArgs.Direction == DirectionTrts.OUT && appArgs.Source != null &&
                        appArgs.Source.ToLower().EndsWith(".tsft"))
                    {
                        isTsftFile = true;
                        tsftFilePath = appArgs.Source;
                    }

                }

                string source = appArgs.Source;
                if (appArgs.Direction == DirectionTrts.IN)
                {
                    source = appArgs.Target;
                }
                if (isTsftFile)
                {
                    String configFile = File.ReadAllText(tsftFilePath, Encoding.UTF8);
                    configFile = StringCipher.Decrypt(configFile, "test");

                    TsftFile tsftFile;
                    using (TextReader reader = new StringReader(configFile))
                    {
                        tsftFile = (TsftFile)new XmlSerializer(typeof(TsftFile)).Deserialize(reader);
                    }

                    if (tsftFile.TempDir.Type == TransferTypes.FTP)
                    {
                        appArgs.TransferType = TransferTypes.FTP;

                    }

                    appArgs.TsftFile = tsftFile;
                    appArgs.FtpUser = appArgs.TsftFile.TempDir.FtpUsername;
                    appArgs.FtpPassword = appArgs.TsftFile.TempDir.FtpPassword;

                    source = appArgs.TsftFile.TempDir.Path;

                }

                if (appArgs.TransferType == TransferTypes.FTP)
                {


                    Uri uriSource = new Uri(source);
                    if (uriSource.Scheme != Uri.UriSchemeFtp)
                    {
                        throw new CliParsingException($"FTP path invalids ('{source}')");
                    }


                    NetworkCredential creds = new NetworkCredential(appArgs.FtpUser, appArgs.FtpPassword);
                    Uri rootUri = UriUtils.GetRootUri(uriSource);
                    if (!FtpUtils.IsOkToConnect(rootUri, creds))
                    {
                        string msgError = $"Cant connect to '{rootUri.AbsoluteUri}'. ";
                        if (isTsftFile)
                        {
                            if (StringUtils.IsNullOrWhiteSpace(appArgs.TsftFile.TempDir.FtpUsername))
                            {
                                msgError +=
                                    "The Tsft file does not provide any credentials. ";
                            }
                            else
                            {
                                msgError +=
                                    "The Tsft file provides credentials, but they did not allow the connection to the remote server. ";
                            }
                        }

                        msgError += $"Specify the -{AppArgsParser.OptFtpUser.ShortOpt} and -{AppArgsParser.OptFtpPassword.ShortOpt} parameters to make the connection possible.";
                        throw new CliParsingException(msgError);
                    }

                    if (appArgs.Direction == DirectionTrts.OUT)
                    {
                        if (!FtpUtils.IsDirectoryExists(uriSource, creds) &&
                            !FtpUtils.IsFileExists(uriSource, creds))
                        {
                            throw new CliParsingException($"FTP path '{source}' must be an existing file or directory");
                        }
                    }
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
                case DirectionTrts.IN:
                    _log.Debug("Go to log-IN.log");
                    _log = new Logger(Path.Combine(AppDataDir, "log-IN.log"), Logger.LogLvl.NONE, Logger.LogLvl.DEBUG, "1 Mo");
                    break;
                case DirectionTrts.OUT:
                    _log.Debug("Go to log-OUT.log");
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
                    case DirectionTrts.IN:
                    {
                        I(_log, "First stage: transfer file from source to temp-shared folder");

                        StartFirstStage(appArgs);
                        break;
                    }
                    case DirectionTrts.OUT:
                    {
                        I(_log, "Second stage: recompose target file from part files from shared folder");

                        StartSecondStage(appArgs);
                        break;
                    }
                    default:
                        argsParser.ShowSyntax();
                        break;
                }
            }
            catch (AppException a)
            {
                Console.WriteLine();
                E(_log, "Error: " + a.Message);
                E(_log, a.StackTrace, false);

                if (appArgs.IsRunByExplorer())
                {
                    Console.Read();
                }

                Environment.Exit(a.ExitCode.Index);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                E(_log, "Error: " + e.Message);
                E(_log, e.StackTrace, false);

                if (appArgs.IsRunByExplorer())
                {
                    Console.Read();
                }

                Environment.Exit(EnumExitCodes.KO.Index);

            }
            finally
            {
                Console.WriteLine();
            }
        }



        private static void StartFirstStage(AppArgs appArgs)
        {
           
            long maxTransferLenght = appArgs.ChunkSize;
            if (appArgs.TransferType == TransferTypes.Windows)
            {
                maxTransferLenght = (long)(FileUtils.GetAvailableSpace(appArgs.Target, 20 * 1024 * 1024) * 0.9);
                I(_log, $"Max size that can be used: {AryxDevLibrary.utils.FileUtils.HumanReadableSize(maxTransferLenght)}");
            } else if (appArgs.TransferType == TransferTypes.FTP)
            {
                //I(_log, $"Max size that can be used: {AryxDevLibrary.utils.FileUtils.HumanReadableSize(maxTransferLenght)}");
            }



            InWorkOptions jobOptions = new InWorkOptions()
            {
                AppArgs = appArgs,
                MaxSizeUsedOnShared = maxTransferLenght,
            };

            AbstractInWork w = null;
            if (appArgs.TransferType == TransferTypes.Windows)
            {
                w = new InToOutWork(jobOptions);
            }
            else if (appArgs.TransferType == TransferTypes.FTP)
            {
                FtpConnexion connexion = new FtpConnexion(new NetworkCredential(appArgs.FtpUser, appArgs.FtpPassword),
                    UriUtils.NewFtpUri(appArgs.Target).GetRootUri());
                w =
                    new FtpInWork(connexion, jobOptions);
            }

            try
            {
                w.DoTransfert();
            }
            catch (Exception ex)
            {
                if (w != null)
                {
                    if (w.LastPartDone > 0)
                    {
                        I(_log, $"You can try resume by adding : --resume-part {(w.LastPartDone + 1)}");
                    }
                }

                throw ex;
            }

        }

        private static void StartSecondStage(AppArgs appArgs)
        {
            OutWorkOptions jobOptions = new OutWorkOptions()
            {
                AppArgs = appArgs,
            };

            AbstractOutWork o = null;
            if (appArgs.TransferType == TransferTypes.Windows)
            {
                o = new OutToFileWork(jobOptions);
            }
            else if (appArgs.TransferType == TransferTypes.FTP)
            {
                o =
                    new FtpOutWork(new NetworkCredential(appArgs.FtpUser, appArgs.FtpPassword),
                        jobOptions);
            }


            o.DoTransfert();
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
