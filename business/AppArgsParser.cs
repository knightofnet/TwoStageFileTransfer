using AryxDevLibrary.utils.cliParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Description = "Force part file size. Default: file size divided by 5, or max 50Mo",
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

        public AppArgsParser()
        {
            AddOption(_optSens);
            AddOption(_optSource);
            AddOption(_optTarget);
            //AddOption(_optDoCompress);
            AddOption(_optBufferSize);
            AddOption(_optChunkSize);

        }

        public override AppArgs ParseDirect(string[] args)
        {
            return Parse(args, ParseTrt);
        }

        private AppArgs ParseTrt(Dictionary<string, Option> arg)
        {
            AppArgs retArgs = new AppArgs();

            retArgs.Direction = GetSingleOptionValue(_optSens, arg).ToUpper();
            if (retArgs.Direction != "IN" && retArgs.Direction != "OUT")
            {
                throw new CliParsingException("Direction must be 'in' or 'out'");
            }


            if (HasOption(_optSource, arg))
            {
                retArgs.Source = GetSingleOptionValue(_optSource, arg);
                if (!File.Exists(retArgs.Source) && !Directory.Exists(retArgs.Source))
                {
                    throw new CliParsingException(string.Format("Source '{0}' must exist", retArgs.Source));
                }
            }

            if (HasOption(_optTarget, arg))
            {
                retArgs.Target = GetSingleOptionValue(_optTarget, arg);
                if (!File.Exists(retArgs.Target) && !Directory.Exists(retArgs.Target))
                {
                    throw new CliParsingException(string.Format("Target '{0}' must exist", retArgs.Target));
                }
            }

            if (HasOption(_optBufferSize, arg))
            {
                string rawBufferSize = GetSingleOptionValue(_optBufferSize, arg);
                if (int.TryParse(rawBufferSize, out var bufferSize))
                {
                    retArgs.BufferSize = bufferSize;
                }
            }

            if (HasOption(_optBufferSize, arg))
            {
                string rawChunkSize = GetSingleOptionValue(_optBufferSize, arg);
                if (int.TryParse(rawChunkSize, out var chunkSize) && chunkSize >= 1024)
                {
                    retArgs.ChunkSize = chunkSize;
                }
                else
                {
                    throw new CliParsingException("Part file size muse be greater or equal than 1024 o");
                }
            }
            else
            {
                retArgs.ChunkSize = -1;
            }

            retArgs.IsDoCompress = HasOption(_optDoCompress, arg);


            return retArgs;


        }
    }
}
