using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.constant;

namespace TwoStageFileTransfer.dto
{
    class AppArgs
    {
        public string Direction { get; internal set; }
        public string Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }

        public bool IsDoCompress { get; internal set; }
        public int ChunkSize { get; internal set; }

        public AppArgs()
        {
            BufferSize = AppCst.BufferSize;
        }
    }
}
