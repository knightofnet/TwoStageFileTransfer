using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils.cliParser;

namespace TwoStageFileTransferCore.constant
{
    public static class CmdArgsOptions
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

        public static readonly Option OptProtocolType = new Option()
        {
            ShortOpt = "p",
            LongOpt = "protocol",
            Description = "Protocol type used for the transfer. Can be Windows, Ftp, Sftp. Default: Windows",
            HasArgs = true,
            Name = "_optProtocolType",
            IsMandatory = false
        };

        public static readonly Option OptSource = new Option()
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

        public static readonly Option OptTarget = new Option()
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

        public static readonly Option OptMaxDiskPlaceToUse = new Option()
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

        public static readonly Option OptChunkSize = new Option()
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

        public static readonly Option OptBufferSize = new Option()
        {
            ShortOpt = "b",
            LongOpt = "buffer-size",
            Description = "Buffer size. Default: " + AppCst.BufferSize,
            HasArgs = true,
            Name = "_optBufferSize",
            IsMandatory = false
        };

        public static readonly Option OptCanOverwrite = new Option()
        {
            ShortOpt = "w",
            LongOpt = "overwrite",
            Description = "Overwrite existing files. Default: none",
            HasArgs = false,
            Name = "_optCanOverwrite",
            IsMandatory = false
        };

        public static readonly Option OptKeepPartFiles = new Option()
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

        public static readonly Option OptFtpUser = new Option()
        {
            ShortOpt = "pu",
            LongOpt = "protocol-username",
            Description = $"Username for connecting to the remote server protocol. Used when -{OptProtocolType.ShortOpt} " +
                          $"is set to Ftp or Sftp. If set, can override username sets in TSFT file. Default: None.",
            HasArgs = true,
            Name = "_optFtpUser",
            IsMandatory = false
        };

        public static readonly Option OptFtpPassword = new Option()
        {
            ShortOpt = "pp",
            LongOpt = "protocol-password",
            Description = $"Password for connecting to the remote server protocol. Used when -{OptProtocolType.ShortOpt} " +
                          "is set to Ftp or Sftp. If set, can override password sets in TSFT file. Default: None.",
            HasArgs = true,
            Name = "_optFtpPassword",
            IsMandatory = false
        };

        public static readonly Option OptIncludeCredsInTsftFile = new Option()
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

        public static readonly Option OptTsftFilePassPhrase = new Option()
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

        public static readonly Option OptTsftPassPhraseNone = new Option()
        {
            ShortOpt = "pn",
            LongOpt = "passphrase-none",
            Description = "Allows you to use the default passphrase.",
            HasArgs = false,
            Name = "_optTsftFilePassPhraseNone",
            IsMandatory = false,
            IsHiddenInHelp = false
        };

        public static readonly Option OptReprise = new Option()
        {
            ShortOpt = "r",
            LongOpt = "resume-part",
            Description = "a",
            HasArgs = true,
            Name = "_optReprise",
            IsMandatory = false,
            IsHiddenInHelp = true
        };
    }
}
