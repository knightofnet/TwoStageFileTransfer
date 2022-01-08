using System;
using System.Collections.Generic;

namespace TwoStageFileTransfer.constant
{
    public sealed class EnumExitCodes
    {
        public static readonly EnumExitCodes OK = new EnumExitCodes(0, "All fine");
        public static readonly EnumExitCodes KO = new EnumExitCodes(1, "Unexpected errors");
        public static readonly EnumExitCodes KO_PARAMS_PARSING = new EnumExitCodes(2, "Error while reading input args");


        public static readonly EnumExitCodes KO_CHECK_BEFORE_TRT = new EnumExitCodes(51, "Error checking pre treatment conditions");

        public static readonly EnumExitCodes KO_IN = new EnumExitCodes(100, "Unexpected error in first stage");
        public static readonly EnumExitCodes KO_WRITING_PARTFILE = new EnumExitCodes(101, "Error while writing part-file");

        public static readonly EnumExitCodes KO_OUT = new EnumExitCodes(200, "Unexpected error in second stage");

        public static IEnumerable<EnumExitCodes> Values
        {
            get
            {
                yield return OK;
                yield return KO;
                yield return KO_PARAMS_PARSING;
                yield return KO_CHECK_BEFORE_TRT;

                yield return KO_IN;
                yield return KO_WRITING_PARTFILE;

                yield return KO_OUT;


            }
        }

        public int Index { get; }
        public String Libelle { get; }


        private EnumExitCodes(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }
    }
}
