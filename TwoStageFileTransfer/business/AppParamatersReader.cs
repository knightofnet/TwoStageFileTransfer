using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.cliParser;
using TwoStageFileTransfer.business.moderuns;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using ProcessUtils = TwoStageFileTransfer.utils.ProcessUtils;
using static TwoStageFileTransferCore.utils.LogUtils;
using AryxDevLibrary.utils.logger;

using TwoStageFileTransferCore.business.connexions;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.exceptions;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransfer.business
{
    class AppParamatersReader
    {
        private static readonly Logger _log = Logger.LastLoggerInstance;
        public CmdAppArgs AppArgs { get; private set; }

        public AppArgsParser ArgsParser { get; private set; }

        public IConnexion Connexion { get; private set; }

        public AbstractModeRun ModeRun { get; private set; }

        public void HydrateAppParameters(string[] args)
        {
            ArgsParser = new AppArgsParser();
            AppArgs = GetAppArgs(args);
        }


        private CmdAppArgs GetAppArgs(string[] args)
        {
            string sourceProcessName = ProcessUtils.ParentProcessUtilities.GetParentProcess().ProcessName.ToLower();

            CmdAppArgs appArgs = null;

            try
            {
                bool isTsftFile = false;
                string tsftFilePath = null;
                if (args.Length == 1 && args[0].ToLower().EndsWith(".tsft"))
                {
                    appArgs = new CmdAppArgs()
                    { Direction = DirectionTrts.OUT, Source = args[0], TransferType = TransferTypes.Windows };
                    isTsftFile = true;
                    tsftFilePath = args[0];
                    D(_log, "Run with tsft file : Mode OUT");

                }
                else
                {
                    appArgs = ArgsParser.ParseDirect(args);
                    D(_log, "Run with parameters");
                    I(_log, $"Args input: {ArgsParser.StrCommand}");

                    if (appArgs.Direction == DirectionTrts.OUT && appArgs.Source != null &&
                        appArgs.Source.ToLower().EndsWith(".tsft"))
                    {
                        isTsftFile = true;
                        tsftFilePath = appArgs.Source;
                    }

                }

                // Controle ChunkSize Vs MaxDiskPlaceToUse
                if (appArgs.MaxDiskPlaceToUse != -1 && appArgs.MaxDiskPlaceToUse < appArgs.ChunkSize)
                {
                    throw new CliParsingException($"Max disk place to use must be greather or equals than chunk size");
                }


                string remotePath = appArgs.Source;
                if (appArgs.Direction == DirectionTrts.IN)
                {
                    remotePath = appArgs.Target;
                }

                if (appArgs.IsRemoteTransfertType && (!UriUtils.IsUriParsable(remotePath) ||
                                                      !IsValidUri(remotePath, appArgs.TransferType)))
                {
                    throw new CliParsingException($"Remote path invalid ('{remotePath}')");
                }


                if (isTsftFile)
                {
                    remotePath = ReadTsftFile(appArgs, tsftFilePath);
                }

                if (appArgs.IsRemoteTransfertType)
                {
                    if (appArgs.TsftPassphrase == null)
                    {
                        appArgs.TsftPassphrase = StringUtils.RandomString(16, AppCst.PassPhraseCharset, true);
                    }

                    Uri uriSource = null;
                    NetworkCredential creds = new NetworkCredential(appArgs.FtpUser, appArgs.FtpPassword);
                    if (appArgs.TransferType == TransferTypes.FTP)
                    {

                        uriSource = new Uri(remotePath);
                        if (uriSource.Scheme != Uri.UriSchemeFtp)
                        {
                            throw new CliParsingException($"Remote path invalid ('{remotePath}') for FTP.");
                        }

                       
                        Uri rootUri = UriUtils.GetRootUri(uriSource);
                        Connexion = new FtpConnexion(creds, rootUri);

                    }
                    else if (appArgs.TransferType == TransferTypes.SFTP)
                    {
                        uriSource = new Uri(remotePath);
                        if (!UriUtils.IsValidSftpUri(uriSource))
                        {
                            throw new CliParsingException($"Remote path invalid ('{remotePath}') for SFTP.");
                        }

                        Connexion = new SshConnexion(creds, uriSource.Host, uriSource.Port);
                    }

                    appArgs.RemotePort = uriSource.Port;

                    if (!Connexion.IsOkToConnect())
                    {
                        string msgError = $"Cant connect to '{uriSource.Host}'. ";
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

                        msgError +=
                            $"Specify the -{CmdArgsOptions.OptFtpUser.ShortOpt} and -{CmdArgsOptions.OptFtpPassword.ShortOpt} parameters to make the connection possible.";
                        throw new CliParsingException(msgError);
                    }

                    if (appArgs.Direction == DirectionTrts.OUT)
                    {
                        if (!Connexion.IsDirectoryExists(uriSource) &&
                            !Connexion.IsFileExists(uriSource))
                        {
                            throw new CliParsingException(
                                $"FTP path '{remotePath}' must be an existing file or directory");
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

                ArgsParser.ShowSyntax();
                if (appArgs == null && "explorer".Equals(sourceProcessName))
                {
                    Console.Read();
                }

                AppUtils.Exit(EnumExitCodes.KO_PARAMS_PARSING);
                return null;
            }
            catch (Exception e)
            {

                I(_log, e.Message);
                if (appArgs == null && "explorer".Equals(sourceProcessName))
                {
                    Console.Read();
                }

                AppUtils.Exit(EnumExitCodes.KO_CHECK_BEFORE_TRT);
                return null;
            }

            appArgs.SourceRun = SourceRuns.Undetermined;
            switch (sourceProcessName)
            {

                case "explorer":
                    appArgs.SourceRun = SourceRuns.Explorer;
                    ModeRun = new ExplorerRun();
                    break;
                case "cmd":
                    appArgs.SourceRun = SourceRuns.CommandPrompt;
                    ModeRun = new CmdRun();
                    break;
                default:
                    appArgs.SourceRun = SourceRuns.CommandPrompt;
                    ModeRun = new CmdRun();
                    break;
            }

            return appArgs;

        }

        private static string ReadTsftFile(AppArgs appArgs, string tsftFilePath)
        {
            if (appArgs.TsftPassphrase == null)
            {
                appArgs.TsftPassphrase = AppCst.DefaultPassPhrase;
            }

            TsftFile tsftFile = CommonAppUtils.DecryptTsft(appArgs, tsftFilePath, appArgs.TransferType != TransferTypes.Windows);
            if (tsftFile == null)
            {
                Console.WriteLine("Enter passphrase to decrypt TSFT file:");
                if (Reader.TryReadLine(out var passphraseInput, 120 * 1000))
                {
                    appArgs.TsftPassphrase = passphraseInput;
                }
                else
                {
                    throw new CliParsingException(
                        $"No passphrase entered to read tsftFile. Use parameter -{CmdArgsOptions.OptTsftFilePassPhrase.ShortOpt} to enter it.");
                }
                tsftFile = CommonAppUtils.DecryptTsft(appArgs, tsftFilePath, true);
            }

            switch (tsftFile.TempDir.Type)
            {
                case TransferTypes.FTP:
                    appArgs.TransferType = TransferTypes.FTP;
                    break;
                case TransferTypes.SFTP:
                    appArgs.TransferType = TransferTypes.SFTP;
                    break;
            }

            appArgs.TsftFile = tsftFile;
            if (string.IsNullOrWhiteSpace(appArgs.FtpUser))
            {
                appArgs.CredentialsOrigin = CredentialOrigins.TsftFile;
                appArgs.FtpUser = appArgs.TsftFile.TempDir.FtpUsername;
                appArgs.FtpPassword = appArgs.TsftFile.TempDir.FtpPassword;
            }

            string remotePath = appArgs.TsftFile.TempDir.Path;


            return remotePath;
        }

 

        private static bool IsValidUri(string remotePath, TransferTypes transferType)
        {
            Uri uriSource;
            switch (transferType)
            {
                case TransferTypes.FTP:
                    {
                        uriSource = new Uri(remotePath);
                        if (uriSource.Scheme != Uri.UriSchemeFtp)
                        {
                            return false;
                        }

                        break;
                    }
                case TransferTypes.SFTP:
                    {
                        uriSource = new Uri(remotePath);
                        if (!UriUtils.IsValidSftpUri(uriSource))
                        {
                            return false;
                        }

                        break;
                    }
            }

            return true;
        }
    }
}
