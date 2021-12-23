using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransfer.constant
{
    public sealed class EnumExitCodes
    {
        public static readonly EnumExitCodes OK = new EnumExitCodes(0, "All fine");
        public static readonly EnumExitCodes KO = new EnumExitCodes(1, "Unexpected errors");
        public static readonly EnumExitCodes KO_PARAMS_PARSING = new EnumExitCodes(2, "Error while reading input args");

        

        public static IEnumerable<EnumExitCodes> Values
        {
            get
            {
                yield return OK;
                yield return KO;
                yield return KO_PARAMS_PARSING;


            }
        }

        public int Index { get; private set; }
        public String Libelle { get; private set; }


        private EnumExitCodes(int index, String libelle)
        {
            Index = index;
            Libelle = libelle;


        }
    }
}
