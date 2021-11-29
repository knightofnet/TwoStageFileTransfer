using AryxDevLibrary.utils.logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransfer.business
{
    abstract class AbstractWork
    {
        protected static Logger _log = Logger.LastLoggerInstance ;

        public FileInfo Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }

    }
}
