using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransferCore.utils.events
{
    public delegate void TsftFileCreated(object sender, TsftFileCreatedArgs args);
    public class TsftFileCreatedArgs
    {
        public string Filepath { get; set; }
        public string Passphrase { get; internal set; }
    }
}
