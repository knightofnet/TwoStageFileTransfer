using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.dto;

namespace TwoStageFileTransfer.business.moderuns
{
    public abstract class AbstractModeRun
    {

        public abstract void InitLogAndAskForParams(AppArgs appArgs);
    }
}
