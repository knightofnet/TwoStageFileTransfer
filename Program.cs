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

        private static AppParamatersReader _appParams;



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

                _appParams = new AppParamatersReader();
                _appParams.HydrateAppParameters(args);

                InitLogAndAskForParams(_appParams.AppArgs);

                I(_log, $"Source: {_appParams.AppArgs.Source}");
                I(_log, $"Target: {_appParams.AppArgs.Target}");

                Console.WriteLine();
                DoWork(_appParams.AppArgs, _appParams.ArgsParser);
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "MainError");
                AppUtils.Exit(EnumExitCodes.KO);
            }

            AppUtils.Exit(EnumExitCodes.OK);
        }

        private static void InitLogAndAskForParams(AppArgs appArgs)
        {
            _appParams.ModeRun.InitLogAndAskForParams(appArgs);



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
                _appParams.Connexion?.Close();
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
            }
            else if (appArgs.TransferType == TransferTypes.FTP)
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
                w = new FtpInWork(_appParams.Connexion, jobOptions);
            }
            else if (appArgs.TransferType == TransferTypes.SFTP)
            {
                w = new SftpInWork(_appParams.Connexion, jobOptions);
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
                o = new FtpOutWork(_appParams.Connexion, jobOptions);
            }
            else if (appArgs.TransferType == TransferTypes.SFTP)
            {
                o = new SftpOutWork(_appParams.Connexion, jobOptions);
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
