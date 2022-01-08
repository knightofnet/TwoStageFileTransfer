using AryxDevLibrary.utils.cliParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransferCore.constant;

namespace TwoStageFileTransfer.business
{
    class AppArgsParser : CliParser<AppArgs>
    {

        public static readonly Option OptSens = new Option()
        {
            ShortOpt = "d",
            LongOpt = "direction",
            Description = "Transfer direction. Values : in,out",
            HasArgs = true,
            Name = "_optSens",
            IsMandatory = true

        };


        internal static readonly Option OptProtocolType = new Option()
        {
            ShortOpt = "p",
            LongOpt = "protocol",
            Description = "Protocol type used for the transfer. Can be Windows, Ftp, Sftp. Default: Windows",
            HasArgs = true,
            Name = "_optProtocolType",
            IsMandatory = false
        };

        internal static readonly Option OptSource = new Option()
        {
            ShortOpt = "s",
            LongOpt = "source",
            Description = "Path to the source file. For the 'in' mode, the file to transfer. For " +
                          "the 'out' mode (i.e. second stage), it depends on the type of the " +
                          "transfert. For 'Windows' type transfers, it can be the first file to be " +
                          "transferred (i.e. the folder containing this first file) or the TSFT file " +
                          "generated at the first level. When it is a transfer using a remote " +
                          "server (type 'FTP' or 'SFTP'), the 'out' mode only accepts a TSFT file " +
                          "as a source file.",
            HasArgs = true,
            Name = "_optSource",
            IsMandatory = false
        };

        internal static readonly Option OptTarget = new Option()
        {
            ShortOpt = "t",
            LongOpt = "target",
            Description = "Path for the target. For the 'in' mode, the folder where to generate the " +
                          "transfer files: this can be a local Windows folder or on the local network, " +
                          "but also a file on an FTP server (uri starting with ftp://) or an SFTP server " +
                          "(uri starting with sftp://host:port). For the latter two cases, this must be " +
                          $"in accordance with the -{OptProtocolType.ShortOpt} parameter. For the 'out' mode, to the folder where " +
                          "the reconstructed file will be placed.",
            HasArgs = true,
            Name = "_optTarget",
            IsMandatory = false
        };

        internal static readonly Option OptMaxDiskPlaceToUse = new Option()
        {
            ShortOpt = "m",
            LongOpt = "maxdiskplace",
            Description = "Maximum size that will be used by all the transfer files. For 'Windows' type " +
                          "transfers, if this parameter is not set, the maximum size will be calculated " +
                          "in relation to the remaining disk space (90% of the remaining space). For " +
                          "transfers using a remote server, the parameter is recommended. If it is omitted, " +
                          "the maximum size used is arbitrarily 20MB.",
            HasArgs = true,
            Name = "_optMaxDiskPlaceToUse",
            IsMandatory = false
        };

        internal static readonly Option OptChunkSize = new Option()
        {
            ShortOpt = "c",
            LongOpt = "chunk",
            Description = "Maximum size of a transfer file (file with a .part extension). " +
                          "Default: size of the available space divided by 10, 50MB max, or the size of " +
                          "the source file.",
            HasArgs = true,
            Name = "_optChunkSize",
            IsMandatory = false
        };



        internal static readonly Option OptBufferSize = new Option()
        {
            ShortOpt = "b",
            LongOpt = "buffer-size",
            Description = "Buffer size. Default: " + AppCst.BufferSize,
            HasArgs = true,
            Name = "_optBufferSize",
            IsMandatory = false
        };

        internal static readonly Option OptCanOverwrite = new Option()
        {
            ShortOpt = "w",
            LongOpt = "overwrite",
            Description = "Overwrite existing files. Default: none",
            HasArgs = false,
            Name = "_optCanOverwrite",
            IsMandatory = false
        };

        internal static readonly Option OptKeepPartFiles = new Option()
        {
            ShortOpt = "k",
            LongOpt = "keep-part-files",
            Description = "Does not delete part files after reading in 'out' mode. " +
                            "Allows to restart the redial process several times, but may prevent the " +
                            "first step from finishing if the maximum size allowed by the transfer is " +
                            "reached and therefore the program is waiting",
            HasArgs = false,
            Name = "_optKeepPartFiles",
            IsMandatory = false
        };



        internal static readonly Option OptFtpUser = new Option()
        {
            ShortOpt = "pu",
            LongOpt = "protocol-username",
            Description = $"Username for connecting to the remote server protocol. Used when -{OptProtocolType.ShortOpt} " +
                          $"is set to Ftp or Sftp. If set, can override username sets in TSFT file. Default: None.",
            HasArgs = true,
            Name = "_optFtpUser",
            IsMandatory = false
        };

        internal static readonly Option OptFtpPassword = new Option()
        {
            ShortOpt = "pp",
            LongOpt = "protocol-password",
            Description = $"Password for connecting to the remote server protocol. Used when -{OptProtocolType.ShortOpt} " +
                          "is set to Ftp or Sftp. If set, can override password sets in TSFT file. Default: None.",
            HasArgs = true,
            Name = "_optFtpPassword",
            IsMandatory = false
        };

        internal static readonly Option OptIncludeCredsInTsftFile = new Option()
        {
            ShortOpt = "pw",
            LongOpt = "tsft-with-credentials",
            Description = "In the 'in' mode, when credentials need to be used (e.g. " +
                          "with the FTP,SFTP protocols), include these data in the generated" +
                          " TSFT file. This way, in the 'out' mode, these data will " +
                          "not be requested again, only the passphrase will be",
            HasArgs = false,
            Name = "OptIncludeCredentialsInTsftFile",
            IsMandatory = false
        };

        internal static readonly Option OptTsftFilePassPhrase = new Option()
        {
            ShortOpt = "ph",
            LongOpt = "passphrase",
            Description = "The passphrase used to encrypt (with 'in' mode) or decrypt (with 'out' mode) " +
                          "the TSFT file. If this parameter is omitted, the passphrase will be randomly " +
                          "generated in 'in' mode (look at the console or logs); for 'out' mode, it will " +
                          "be requested.",
            HasArgs = true,
            Name = "_optTsftFilePassPhrase",
            IsMandatory = false,
            IsHiddenInHelp = false
        };

        internal static readonly Option OptTsftPassPhraseNone = new Option()
        {
            ShortOpt = "pn",
            LongOpt = "passphrase-none",
            Description = "Allows you to use the default passphrase.",
            HasArgs = false,
            Name = "_optTsftFilePassPhraseNone",
            IsMandatory = false,
            IsHiddenInHelp = false
        };



        internal static readonly Option OptReprise = new Option()
        {
            ShortOpt = "r",
            LongOpt = "resume-part",
            Description = "a",
            HasArgs = true,
            Name = "_optReprise",
            IsMandatory = false,
            IsHiddenInHelp = true
        };





        public AppArgsParser()
        {
            AddOption(OptSens);
            AddOption(OptSource);
            AddOption(OptTarget);
            //AddOption(_optDoCompress);
            AddOption(OptBufferSize);
            AddOption(OptMaxDiskPlaceToUse);
            AddOption(OptChunkSize);
            AddOption(OptCanOverwrite);
            AddOption(OptKeepPartFiles);


            AddOption(OptReprise);



            AddOption(OptProtocolType);
            AddOption(OptFtpUser);
            AddOption(OptFtpPassword);



            AddOption(OptIncludeCredsInTsftFile);

            AddOption(OptTsftFilePassPhrase);
            AddOption(OptTsftPassPhraseNone);


        }

        public StringBuilder StrCommand { get; set; }

        public override AppArgs ParseDirect(string[] args)
        {
            return Parse(args, ParseTrt);
        }

        private AppArgs ParseTrt(Dictionary<string, Option> arg)
        {
            AppArgs retArgs = new AppArgs();
            StrCommand = new StringBuilder(Path.GetFileName(Assembly.GetExecutingAssembly().Location) + " ");

            // Direction
            string rawSource = GetSingleOptionValue(OptSens, arg, "NONE");
            retArgs.Direction = (DirectionTrts)Enum.Parse(typeof(DirectionTrts), rawSource, true);
            if (retArgs.Direction != DirectionTrts.IN && retArgs.Direction != DirectionTrts.OUT)
            {
                throw new CliParsingException("Direction must be 'in' or 'out'");
            }
            StrCommand.AppendFormat("-{0} {1} ", OptSens.ShortOpt, retArgs.Direction);


            // TransferType
            string rawTransfertType =
                GetSingleOptionValue(OptProtocolType, arg, TransferTypes.Windows.ToString());
            retArgs.TransferType = (TransferTypes)Enum.Parse(typeof(TransferTypes), rawTransfertType, true);
            if (HasOption(OptProtocolType, arg))
            {
                StrCommand.AppendFormat("-{0} {1} ", OptProtocolType.ShortOpt, retArgs.TransferType);
            }


            // Username and password for FTP
            if (HasOption(OptFtpUser, arg) || HasOption(OptFtpPassword, arg))
            {
                if (HasOption(OptFtpUser, arg))
                {
                    retArgs.FtpUser = GetSingleOptionValue(OptFtpUser, arg);
                    StrCommand.AppendFormat("-{0} {1} ", OptFtpUser.ShortOpt, retArgs.FtpUser);
                    retArgs.CredentialsOrigin = CredentialOrigins.InputParameters;
                }

                if (HasOption(OptFtpPassword, arg))
                {
                    retArgs.FtpPassword = GetSingleOptionValue(OptFtpPassword, arg);
                    StrCommand.AppendFormat("-{0} {1} ", OptFtpPassword.ShortOpt, "***");
                }
            }


            // Source
            if (HasOption(OptSource, arg))
            {
                retArgs.Source = GetSingleOptionValue(OptSource, arg);

                if (retArgs.Direction == DirectionTrts.IN)
                {
                    if (!FileUtils.IsAFile(retArgs.Source) || !File.Exists(retArgs.Source))
                    {
                        throw new CliParsingException($"Source '{retArgs.Source}' must be an existing file");
                    }

                    retArgs.Source = Path.GetFullPath(retArgs.Source);
                }
                else if (retArgs.Direction == DirectionTrts.OUT && retArgs.TransferType == TransferTypes.Windows)
                {
                    if (!File.Exists(retArgs.Source) && !Directory.Exists(retArgs.Source))
                    {
                        throw new CliParsingException($"Source '{retArgs.Source}' must be an existing file or directory");
                    }
                    retArgs.Source = Path.GetFullPath(retArgs.Source);
                }
                else if (retArgs.Direction == DirectionTrts.OUT && retArgs.IsRemoteTransfertType)
                {
                    if (FileUtils.IsValidFilepath(retArgs.Source) && !retArgs.Source.ToLower().EndsWith(".tsft"))
                    {
                        throw new CliParsingException($"If source is a file, it must be a valid tsft file.");
                    }

                }


                StrCommand.AppendFormat("-{0} \"{1}\" ", OptSource.ShortOpt, retArgs.Source);
            }


            // Target
            if (HasOption(OptTarget, arg))
            {
                retArgs.Target = GetSingleOptionValue(OptTarget, arg);


                if (retArgs.TransferType == TransferTypes.Windows)
                {
                    retArgs.Target = Path.GetFullPath(retArgs.Target);

                }
                else if (retArgs.Direction == DirectionTrts.IN && retArgs.IsRemoteTransfertType)
                {

                }

                StrCommand.AppendFormat("-{0} \"{1}\" ", OptTarget.ShortOpt, retArgs.Target);
            }


            // BufferSize
            if (HasOption(OptBufferSize, arg))
            {
                string rawBufferSize = GetSingleOptionValue(OptBufferSize, arg);
                if (int.TryParse(rawBufferSize, out var bufferSize))
                {
                    retArgs.BufferSize = bufferSize;
                    StrCommand.AppendFormat("-{0} {1} ", OptBufferSize.ShortOpt, FileUtils.HumanReadableSize(retArgs.BufferSize));
                }

            }

            // MaxDiskPlaceToUse
            if (HasOption(OptMaxDiskPlaceToUse, arg))
            {
                string rawMaxDiskPlaceToUse = GetSingleOptionValue(OptMaxDiskPlaceToUse, arg);
                long res = (long)FileUtils.HumanReadableSizeToLong(rawMaxDiskPlaceToUse);
                if (res != -1)
                {
                    retArgs.MaxDiskPlaceToUse = res;
                }
                else if (long.TryParse(rawMaxDiskPlaceToUse, out var maxDiskPlaceToUse))
                {
                    retArgs.MaxDiskPlaceToUse = maxDiskPlaceToUse;
                }

                if (retArgs.MaxDiskPlaceToUse < FileUtils.HumanReadableSizeToLong("1Mo"))
                {
                    throw new CliParsingException("Max disk place to use must be greater or equals to 1Mo.");
                }
                StrCommand.AppendFormat("-{0} {1} ", OptMaxDiskPlaceToUse.ShortOpt, FileUtils.HumanReadableSize(retArgs.MaxDiskPlaceToUse));
            }
            else
            {
                retArgs.MaxDiskPlaceToUse = -1;
            }


            // ChunkSize
            if (HasOption(OptChunkSize, arg))
            {
                string rawChunkSize = GetSingleOptionValue(OptChunkSize, arg);
                long res = (long)FileUtils.HumanReadableSizeToLong(rawChunkSize);
                if (res != -1)
                {
                    retArgs.ChunkSize = res;
                }
                else if (long.TryParse(rawChunkSize, out var chunkSize))
                {
                    retArgs.ChunkSize = chunkSize;
                }

                if (retArgs.ChunkSize < 1024)
                {
                    throw new CliParsingException("Part file size muse be greater or equal than 1024 o");
                }
                StrCommand.AppendFormat("-{0} {1} ", OptChunkSize.ShortOpt, FileUtils.HumanReadableSize(retArgs.ChunkSize));

            }
            else
            {
                retArgs.ChunkSize = -1;
            }




            if (HasOption(OptCanOverwrite, arg))
            {
                retArgs.CanOverwrite = HasOption(OptCanOverwrite, arg);
                StrCommand.AppendFormat("-{0} ", OptCanOverwrite.ShortOpt);
            }

            if (HasOption(OptIncludeCredsInTsftFile, arg))
            {
                retArgs.IncludeCredsInTsft = HasOption(OptIncludeCredsInTsftFile, arg);
                StrCommand.AppendFormat("-{0} ", OptIncludeCredsInTsftFile.ShortOpt);
            }

            if (HasOption(OptKeepPartFiles, arg))
            {
                retArgs.KeepPartFiles = HasOption(OptKeepPartFiles, arg);
                StrCommand.AppendFormat("-{0} ", OptKeepPartFiles.ShortOpt);
            }

            if (HasOption(OptTsftFilePassPhrase, arg) && HasOption(OptTsftPassPhraseNone, arg))
            {
                throw new CliParsingException($"-{OptTsftFilePassPhrase.ShortOpt} and -{OptTsftPassPhraseNone.ShortOpt} cannot " +
                                              $"be used at the same time. Also, -{OptTsftPassPhraseNone.ShortOpt} only works with " +
                                              $"the \"{TransferTypes.Windows}\" transfer type.");
            }

            // OptTsftFilePassPhrase
            if (HasOption(OptTsftFilePassPhrase, arg))
            {
                retArgs.TsftPassphrase = GetSingleOptionValue(OptTsftFilePassPhrase, arg);
                StrCommand.AppendFormat("-{0} {1} ", OptTsftFilePassPhrase.ShortOpt, retArgs.TsftPassphrase);
            }

            // OptTsftPassPhraseNone
            if (HasOption(OptTsftPassPhraseNone, arg))
            {
                retArgs.TsftPassphrase = AppCst.DefaultPassPhrase;
                StrCommand.AppendFormat("-{0} ", OptTsftPassPhraseNone.ShortOpt);
            }


            if (HasOption(OptReprise, arg))
            {
                retArgs.ResumePart = GetSingleOptionValueInt(OptReprise, arg, 0);
                if (retArgs.ResumePart < 0)
                {
                    throw new CliParsingException("Resume part must be positive or equal to zero");
                }
                StrCommand.AppendFormat("-{0} {1} ", OptReprise.ShortOpt, retArgs.ResumePart);
            }



            return retArgs;


        }


    }
}
