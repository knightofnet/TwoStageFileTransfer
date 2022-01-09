using System;
using System.IO;
using System.Reflection;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using TwoStageFileTransfer.business;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;

using TwoStageFileTransfer.utils;
using TwoStageFileTransferCore.business.transfer;
using TwoStageFileTransferCore.business.transfer.firststage;
using TwoStageFileTransferCore.business.transfer.secondstage;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.exceptions;
using TwoStageFileTransferCore.utils;
using static TwoStageFileTransferCore.utils.LogUtils;
using FileUtils = TwoStageFileTransferCore.utils.FileUtils;
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

        private static void DoWork(CmdAppArgs appArgs, AppArgsParser argsParser)
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
            catch (CommonAppException a)
            {
                Console.WriteLine();
                E(_log, "Error: " + a.Message);
                E(_log, a.StackTrace, false);

                if (appArgs.IsRunByExplorer())
                {
                    Console.Read();
                }

                EnumExitCodes exitCodes = EnumExitCodes.KO;
                switch (a.ExceptReason)
                {
                    case CommonAppExceptReason.ErrorInStage:
                        exitCodes = EnumExitCodes.KO_IN;
                        break;
                    case CommonAppExceptReason.ErrorOutStage:
                        exitCodes = EnumExitCodes.KO_OUT;
                        break;
                    case CommonAppExceptReason.ErrorCheckParams:
                        exitCodes = EnumExitCodes.KO_PARAMS_PARSING;
                        break;
                    case CommonAppExceptReason.ErrorInStageWritingPartFile:
                        exitCodes = EnumExitCodes.KO_WRITING_PARTFILE;
                        break;
                    case CommonAppExceptReason.ErrorPreparingTreatment:
                        exitCodes = EnumExitCodes.KO_CHECK_BEFORE_TRT;
                        break;
                }

                Environment.Exit(exitCodes.Index);
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

            long maxTransferLenght = appArgs.MaxDiskPlaceToUse;
            if (maxTransferLenght == -1)
            {
                if (appArgs.TransferType == TransferTypes.Windows)
                {
                    maxTransferLenght = (long)(FileUtils.GetAvailableSpace(appArgs.Target, 20 * 1024 * 1024) * 0.9);
                    I(_log, $"Max size that can be used: {AryxDevLibrary.utils.FileUtils.HumanReadableSize(maxTransferLenght)}");
                }
                else if (appArgs.IsRemoteTransfertType)
                {
                    maxTransferLenght = (long)AryxDevLibrary.utils.FileUtils.HumanReadableSizeToLong("20Mo");
                }
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
                using (ProgressBar pbar = new ProgressBar())
                    w.DoTransfert(pbar);
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

            using (ProgressBar pbar = new ProgressBar())
                o.DoTransfert(pbar);

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
