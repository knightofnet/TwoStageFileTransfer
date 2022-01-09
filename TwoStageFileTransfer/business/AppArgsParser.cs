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
    class AppArgsParser : CliParser<CmdAppArgs>
    {
        public AppArgsParser()
        {
            AddOption(CmdArgsOptions.OptSens);
            AddOption(CmdArgsOptions.OptSource);
            AddOption(CmdArgsOptions.OptTarget);
            //AddOption(_optDoCompress);
            AddOption(CmdArgsOptions.OptBufferSize);
            AddOption(CmdArgsOptions.OptMaxDiskPlaceToUse);
            AddOption(CmdArgsOptions.OptChunkSize);
            AddOption(CmdArgsOptions.OptCanOverwrite);
            AddOption(CmdArgsOptions.OptKeepPartFiles);


            AddOption(CmdArgsOptions.OptReprise);



            AddOption(CmdArgsOptions.OptProtocolType);
            AddOption(CmdArgsOptions.OptFtpUser);
            AddOption(CmdArgsOptions.OptFtpPassword);



            AddOption(CmdArgsOptions.OptIncludeCredsInTsftFile);

            AddOption(CmdArgsOptions.OptTsftFilePassPhrase);
            AddOption(CmdArgsOptions.OptTsftPassPhraseNone);


        }

        public StringBuilder StrCommand { get; set; }

        public override CmdAppArgs ParseDirect(string[] args)
        {
            return Parse(args, ParseTrt);
        }

        private CmdAppArgs ParseTrt(Dictionary<string, Option> arg)
        {
            CmdAppArgs retArgs = new CmdAppArgs();
            StrCommand = new StringBuilder(Path.GetFileName(Assembly.GetExecutingAssembly().Location) + " ");

            // Direction
            string rawSource = GetSingleOptionValue(CmdArgsOptions.OptSens, arg, "NONE");
            retArgs.Direction = (DirectionTrts)Enum.Parse(typeof(DirectionTrts), rawSource, true);
            if (retArgs.Direction != DirectionTrts.IN && retArgs.Direction != DirectionTrts.OUT)
            {
                throw new CliParsingException("Direction must be 'in' or 'out'");
            }
            StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptSens.ShortOpt, retArgs.Direction);


            // TransferType
            string rawTransfertType =
                GetSingleOptionValue(CmdArgsOptions.OptProtocolType, arg, TransferTypes.Windows.ToString());
            retArgs.TransferType = (TransferTypes)Enum.Parse(typeof(TransferTypes), rawTransfertType, true);
            if (HasOption(CmdArgsOptions.OptProtocolType, arg))
            {
                StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptProtocolType.ShortOpt, retArgs.TransferType);
            }


            // Username and password for FTP
            if (HasOption(CmdArgsOptions.OptFtpUser, arg) || HasOption(CmdArgsOptions.OptFtpPassword, arg))
            {
                if (HasOption(CmdArgsOptions.OptFtpUser, arg))
                {
                    retArgs.FtpUser = GetSingleOptionValue(CmdArgsOptions.OptFtpUser, arg);
                    StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptFtpUser.ShortOpt, retArgs.FtpUser);
                    retArgs.CredentialsOrigin = CredentialOrigins.InputParameters;
                }

                if (HasOption(CmdArgsOptions.OptFtpPassword, arg))
                {
                    retArgs.FtpPassword = GetSingleOptionValue(CmdArgsOptions.OptFtpPassword, arg);
                    StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptFtpPassword.ShortOpt, "***");
                }
            }


            // Source
            if (HasOption(CmdArgsOptions.OptSource, arg))
            {
                retArgs.Source = GetSingleOptionValue(CmdArgsOptions.OptSource, arg);

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


                StrCommand.AppendFormat("-{0} \"{1}\" ", CmdArgsOptions.OptSource.ShortOpt, retArgs.Source);
            }


            // Target
            if (HasOption(CmdArgsOptions.OptTarget, arg))
            {
                retArgs.Target = GetSingleOptionValue(CmdArgsOptions.OptTarget, arg);


                if (retArgs.TransferType == TransferTypes.Windows)
                {
                    retArgs.Target = Path.GetFullPath(retArgs.Target);

                }
                else if (retArgs.Direction == DirectionTrts.IN && retArgs.IsRemoteTransfertType)
                {

                }

                StrCommand.AppendFormat("-{0} \"{1}\" ", CmdArgsOptions.OptTarget.ShortOpt, retArgs.Target);
            }


            // BufferSize
            if (HasOption(CmdArgsOptions.OptBufferSize, arg))
            {
                string rawBufferSize = GetSingleOptionValue(CmdArgsOptions.OptBufferSize, arg);
                if (int.TryParse(rawBufferSize, out var bufferSize))
                {
                    retArgs.BufferSize = bufferSize;
                    StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptBufferSize.ShortOpt, FileUtils.HumanReadableSize(retArgs.BufferSize));
                }

            }

            // MaxDiskPlaceToUse
            if (HasOption(CmdArgsOptions.OptMaxDiskPlaceToUse, arg))
            {
                string rawMaxDiskPlaceToUse = GetSingleOptionValue(CmdArgsOptions.OptMaxDiskPlaceToUse, arg);
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
                StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptMaxDiskPlaceToUse.ShortOpt, FileUtils.HumanReadableSize(retArgs.MaxDiskPlaceToUse));
            }
            else
            {
                retArgs.MaxDiskPlaceToUse = -1;
            }


            // ChunkSize
            if (HasOption(CmdArgsOptions.OptChunkSize, arg))
            {
                string rawChunkSize = GetSingleOptionValue(CmdArgsOptions.OptChunkSize, arg);
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
                StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptChunkSize.ShortOpt, FileUtils.HumanReadableSize(retArgs.ChunkSize));

            }
            else
            {
                retArgs.ChunkSize = -1;
            }




            if (HasOption(CmdArgsOptions.OptCanOverwrite, arg))
            {
                retArgs.CanOverwrite = HasOption(CmdArgsOptions.OptCanOverwrite, arg);
                StrCommand.AppendFormat("-{0} ", CmdArgsOptions.OptCanOverwrite.ShortOpt);
            }

            if (HasOption(CmdArgsOptions.OptIncludeCredsInTsftFile, arg))
            {
                retArgs.IncludeCredsInTsft = HasOption(CmdArgsOptions.OptIncludeCredsInTsftFile, arg);
                StrCommand.AppendFormat("-{0} ", CmdArgsOptions.OptIncludeCredsInTsftFile.ShortOpt);
            }

            if (HasOption(CmdArgsOptions.OptKeepPartFiles, arg))
            {
                retArgs.KeepPartFiles = HasOption(CmdArgsOptions.OptKeepPartFiles, arg);
                StrCommand.AppendFormat("-{0} ", CmdArgsOptions.OptKeepPartFiles.ShortOpt);
            }

            if (HasOption(CmdArgsOptions.OptTsftFilePassPhrase, arg) && HasOption(CmdArgsOptions.OptTsftPassPhraseNone, arg))
            {
                throw new CliParsingException($"-{CmdArgsOptions.OptTsftFilePassPhrase.ShortOpt} and -{CmdArgsOptions.OptTsftPassPhraseNone.ShortOpt} cannot " +
                                              $"be used at the same time. Also, -{CmdArgsOptions.OptTsftPassPhraseNone.ShortOpt} only works with " +
                                              $"the \"{TransferTypes.Windows}\" transfer type.");
            }

            // OptTsftFilePassPhrase
            if (HasOption(CmdArgsOptions.OptTsftFilePassPhrase, arg))
            {
                retArgs.TsftPassphrase = GetSingleOptionValue(CmdArgsOptions.OptTsftFilePassPhrase, arg);
                StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptTsftFilePassPhrase.ShortOpt, retArgs.TsftPassphrase);
            }

            // OptTsftPassPhraseNone
            if (HasOption(CmdArgsOptions.OptTsftPassPhraseNone, arg))
            {
                retArgs.TsftPassphrase = AppCst.DefaultPassPhrase;
                StrCommand.AppendFormat("-{0} ", CmdArgsOptions.OptTsftPassPhraseNone.ShortOpt);
            }


            if (HasOption(CmdArgsOptions.OptReprise, arg))
            {
                retArgs.ResumePart = GetSingleOptionValueInt(CmdArgsOptions.OptReprise, arg, 0);
                if (retArgs.ResumePart < 0)
                {
                    throw new CliParsingException("Resume part must be positive or equal to zero");
                }
                StrCommand.AppendFormat("-{0} {1} ", CmdArgsOptions.OptReprise.ShortOpt, retArgs.ResumePart);
            }



            return retArgs;


        }


    }
}
