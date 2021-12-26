using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransfer.dto
{
    class InWorkOptions : CommonWorkOptions
    {
        public long MaxSizeUsedOnShared { get; set; }

        public long PartFileSize { get; set; }


    }
}
