using AryxDevLibrary.utils.cliParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;

namespace TwoStageFileTransfer.business
{
    class AppArgsParser : CliParser<AppArgs>
    {

        private static readonly Option _optSens = new Option()
        {
            ShortOpt = "d",
            LongOpt = "direction",
            Description = "Transfer direction. Values : in,out",
            HasArgs = true,
            Name = "_optSens",
            IsMandatory = true

        };

        private static readonly Option _optSource = new Option()
        {
            ShortOpt = "s",
            LongOpt = "source",
            Description = "Path to source file. For the 'in' mode, the file to be transfered. For the 'out' mode the first transfert file (or the folder containing this first file)",
            HasArgs = true,
            Name = "_optSource",
            IsMandatory = false
        };

        private static readonly Option _optTarget = new Option()
        {
            ShortOpt = "t",
            LongOpt = "target",
            Description = "Path to the target. For the 'in' mode, to the folder where to generate the transfer files. for the 'out' mode, the folder where the file will be placed",
            HasArgs = true,
            Name = "_optTarget",
            IsMandatory = false
        };

        private static readonly Option _optChunkSize = new Option()
        {
            ShortOpt = "c",
            LongOpt = "chunk",
            Description = "Force part file size. Default: file size divided by 10, or max 50Mo",
            HasArgs = true,
            Name = "_optChunkSize",
            IsMandatory = false
        };

        private static readonly Option _optDoCompress = new Option()
        {
            ShortOpt = "dc",
            LongOpt = "compress-before",
            Description = "Compress file before transfert. Default: none",
            HasArgs = false,
            Name = "_optDoCompress",
            IsMandatory = false
        };

        private static readonly Option _optBufferSize = new Option()
        {
            ShortOpt = "b",
            LongOpt = "buffer-size",
            Description = "Buffer size. Default: " + AppCst.BufferSize,
            HasArgs = true,
            Name = "_optBufferSize",
            IsMandatory = false
        };

        private static readonly Option _optCanOverwrite = new Option()
        {
            ShortOpt = "w",
            LongOpt = "overwrite",
            Description = "Overwrite existing files. Default: none",
            HasArgs = false,
            Name = "_optCanOverwrite",
            IsMandatory = false
        };

        public AppArgsParser()
        {
            AddOption(_optSens);
            AddOption(_optSource);
            AddOption(_optTarget);
            //AddOption(_optDoCompress);
            AddOption(_optBufferSize);
            AddOption(_optChunkSize);
            AddOption(_optCanOverwrite);

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
            

            retArgs.Direction = GetSingleOptionValue(_optSens, arg).ToUpper();
            if (retArgs.Direction != AppCst.MODE_IN && retArgs.Direction != AppCst.MODE_OUT)
            {
                throw new CliParsingException("Direction must be 'in' or 'out'");
            }
            StrCommand.AppendFormat("--{0} {1} ", _optSens.LongOpt, retArgs.Direction);


            if (HasOption(_optSource, arg))
            {
                retArgs.Source = GetSingleOptionValue(_optSource, arg);
                if (!File.Exists(retArgs.Source) && !Directory.Exists(retArgs.Source))
                {
                    throw new CliParsingException($"Source '{retArgs.Source}' must exist");
                }
                StrCommand.AppendFormat("--{0} {1} ", _optSource.LongOpt, retArgs.Source);
            }

            if (HasOption(_optTarget, arg))
            {
                retArgs.Target = GetSingleOptionValue(_optTarget, arg);
                if (!File.Exists(retArgs.Target) && !Directory.Exists(retArgs.Target))
                {
                    throw new CliParsingException($"Target '{retArgs.Target}' must exist");
                }
                StrCommand.AppendFormat("--{0} {1} ", _optTarget.LongOpt, retArgs.Target);
            }

            if (HasOption(_optBufferSize, arg))
            {
                string rawBufferSize = GetSingleOptionValue(_optBufferSize, arg);
                if (int.TryParse(rawBufferSize, out var bufferSize))
                {
                    retArgs.BufferSize = bufferSize;
                    StrCommand.AppendFormat("--{0} {1} ", _optBufferSize.LongOpt, retArgs.BufferSize);
                }
                
            }

            if (HasOption(_optChunkSize, arg))
            {
                string rawChunkSize = GetSingleOptionValue(_optChunkSize, arg);
                long res = (long)AryxDevLibrary.utils.FileUtils.HumanReadableSizeToLong(rawChunkSize);
                if (res != -1 )
                {                    
                    retArgs.ChunkSize = res;
                }
                else if (long.TryParse(rawChunkSize, out var chunkSize))
                {
                    retArgs.ChunkSize = chunkSize;
                }

                if (retArgs.ChunkSize  < 1024)                
                {
                    throw new CliParsingException("Part file size muse be greater or equal than 1024 o");
                }
                StrCommand.AppendFormat("--{0} {1} ", _optChunkSize.LongOpt, retArgs.ChunkSize);

            }
            else
            {
                retArgs.ChunkSize = -1;
            }

            if (HasOption(_optDoCompress, arg))
            {
                retArgs.IsDoCompress = HasOption(_optDoCompress, arg);
                StrCommand.AppendFormat("--{0} ", _optDoCompress.LongOpt, retArgs.IsDoCompress);
            }

            if (HasOption(_optCanOverwrite, arg))
            {
                retArgs.CanOverwrite = HasOption(_optCanOverwrite, arg);
                StrCommand.AppendFormat("{0} ", _optCanOverwrite.LongOpt, retArgs.CanOverwrite);
            }

            return retArgs;


        }

        
    }
}
