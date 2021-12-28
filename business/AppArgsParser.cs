using AryxDevLibrary.utils.cliParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;

namespace TwoStageFileTransfer.business
{
    class AppArgsParser : CliParser<AppArgs>
    {

        private static readonly Option OptSens = new Option()
        {
            ShortOpt = "d",
            LongOpt = "direction",
            Description = "Transfer direction. Values : in,out",
            HasArgs = true,
            Name = "_optSens",
            IsMandatory = true

        };

        private static readonly Option OptSource = new Option()
        {
            ShortOpt = "s",
            LongOpt = "source",
            Description = "Path to source file. For the 'in' mode, the file to be transfered. For the 'out' mode the first transfert file (or the folder containing this first file)",
            HasArgs = true,
            Name = "_optSource",
            IsMandatory = false
        };

        private static readonly Option OptTarget = new Option()
        {
            ShortOpt = "t",
            LongOpt = "target",
            Description = "Path to the target. For the 'in' mode, to the folder where to generate the transfer files. for the 'out' mode, the folder where the file will be placed",
            HasArgs = true,
            Name = "_optTarget",
            IsMandatory = false
        };

        private static readonly Option OptChunkSize = new Option()
        {
            ShortOpt = "c",
            LongOpt = "chunk",
            Description = "Force part file size. Default: file size divided by 10, or max 50Mo",
            HasArgs = true,
            Name = "_optChunkSize",
            IsMandatory = false
        };

        private static readonly Option OptDoCompress = new Option()
        {
            ShortOpt = "dc",
            LongOpt = "compress-before",
            Description = "Compress file before transfert. Default: none",
            HasArgs = false,
            Name = "_optDoCompress",
            IsMandatory = false
        };

        private static readonly Option OptBufferSize = new Option()
        {
            ShortOpt = "b",
            LongOpt = "buffer-size",
            Description = "Buffer size. Default: " + AppCst.BufferSize,
            HasArgs = true,
            Name = "_optBufferSize",
            IsMandatory = false
        };

        private static readonly Option OptCanOverwrite = new Option()
        {
            ShortOpt = "w",
            LongOpt = "overwrite",
            Description = "Overwrite existing files. Default: none",
            HasArgs = false,
            Name = "_optCanOverwrite",
            IsMandatory = false
        };

        private static readonly Option OptProtocolType = new Option()
        {
            ShortOpt = "p",
            LongOpt = "protocol",
            Description = "Protocol: Windows,Ftp. Default: Windows",
            HasArgs = true,
            Name = "_optProtocolType",
            IsMandatory = false
        };


        private static readonly Option OptFtpUser = new Option()
        {
            ShortOpt = "pu",
            LongOpt = "protocol-username",
            Description = "Username for connecting to the protocol (ex: FTP)",
            HasArgs = true,
            Name = "_optFtpUser",
            IsMandatory = false
        };

        private static readonly Option OptFtpPassword = new Option()
        {
            ShortOpt = "pp",
            LongOpt = "protocol-password",
            Description = "Password for connecting to the protocol (ex: FTP)",
            HasArgs = true,
            Name = "_optFtpPassword",
            IsMandatory = false
        };


        private static readonly Option OptReprise = new Option()
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
            AddOption(OptChunkSize);
            AddOption(OptCanOverwrite);

            AddOption(OptReprise);


            AddOption(OptProtocolType);
            AddOption(OptFtpUser);
            AddOption(OptFtpPassword);

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
            StrCommand.AppendFormat("--{0} {1} ", OptSens.LongOpt, retArgs.Direction);


            // TransferType
            string rawTransfertType =
                GetSingleOptionValue(OptProtocolType, arg, TransferTypes.WindowsFolder.ToString());
            retArgs.TransferType = (TransferTypes)Enum.Parse(typeof(TransferTypes), rawTransfertType, true);
            if (HasOption(OptProtocolType, arg))
            {
                StrCommand.AppendFormat("--{0} {1} ", OptProtocolType.LongOpt, retArgs.TransferType);
            }


            // Username and password for FTP
            if (retArgs.TransferType == TransferTypes.FTP &&
                (HasOption(OptFtpUser, arg) || HasOption(OptFtpPassword, arg))
            )
            {
                if (HasOption(OptFtpUser, arg))
                {
                    retArgs.FtpUser = GetSingleOptionValue(OptFtpUser, arg);
                    StrCommand.AppendFormat("--{0} {1} ", OptFtpUser.LongOpt, retArgs.FtpUser);
                }

                if (HasOption(OptFtpPassword, arg))
                {
                    retArgs.FtpPassword = GetSingleOptionValue(OptFtpPassword, arg);
                    StrCommand.AppendFormat("--{0} {1} ", OptFtpPassword.LongOpt, "***");
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
                else if (retArgs.Direction == DirectionTrts.OUT && retArgs.TransferType == TransferTypes.WindowsFolder)
                {
                    if (!File.Exists(retArgs.Source) && !Directory.Exists(retArgs.Source))
                    {
                        throw new CliParsingException($"Source '{retArgs.Source}' must be an existing file or directory");
                    }
                    retArgs.Source = Path.GetFullPath(retArgs.Source);
                }
                else if (retArgs.Direction == DirectionTrts.OUT && retArgs.TransferType == TransferTypes.FTP)
                {
                    if (FileUtils.IsValidFilepath(retArgs.Source) && !retArgs.Source.ToLower().EndsWith(".tsft"))
                    {
                        throw new CliParsingException($"If source is a file, it must be a valid tsft file.");
                    }


                }


                StrCommand.AppendFormat("--{0} {1} ", OptSource.LongOpt, retArgs.Source);
            }


            // Target
            if (HasOption(OptTarget, arg))
            {
                retArgs.Target = GetSingleOptionValue(OptTarget, arg);
                

                if (retArgs.TransferType == TransferTypes.WindowsFolder)
                {
                    retArgs.Target = Path.GetFullPath(retArgs.Target);

                } else if (retArgs.Direction == DirectionTrts.IN && retArgs.TransferType == TransferTypes.FTP)
                {



                }

                StrCommand.AppendFormat("--{0} {1} ", OptTarget.LongOpt, retArgs.Target);
            }


            // BufferSize
            if (HasOption(OptBufferSize, arg))
            {
                string rawBufferSize = GetSingleOptionValue(OptBufferSize, arg);
                if (int.TryParse(rawBufferSize, out var bufferSize))
                {
                    retArgs.BufferSize = bufferSize;
                    StrCommand.AppendFormat("--{0} {1} ", OptBufferSize.LongOpt, retArgs.BufferSize);
                }

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
                StrCommand.AppendFormat("--{0} {1} ", OptChunkSize.LongOpt, retArgs.ChunkSize);

            }
            else
            {
                retArgs.ChunkSize = -1;
            }


            if (HasOption(OptDoCompress, arg))
            {
                retArgs.IsDoCompress = HasOption(OptDoCompress, arg);
                StrCommand.AppendFormat("--{0} ", OptDoCompress.LongOpt);
            }

            if (HasOption(OptCanOverwrite, arg))
            {
                retArgs.CanOverwrite = HasOption(OptCanOverwrite, arg);
                StrCommand.AppendFormat("{0} ", OptCanOverwrite.LongOpt);
            }


            if (HasOption(OptReprise, arg))
            {
                retArgs.ResumePart = GetSingleOptionValueInt(OptReprise, arg, 0);
                if (retArgs.ResumePart < 0)
                {
                    throw new CliParsingException("Resume part must be positive or equal to zero");
                }
                StrCommand.AppendFormat("--{0} {1} ", OptReprise.LongOpt, retArgs.ResumePart);
            }

            return retArgs;


        }


    }
}
