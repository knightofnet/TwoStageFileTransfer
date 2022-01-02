using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransfer.constant
{
    public enum CredentialOrigins
    {
        Undetermined = 0,
        TsftFile = 1,
        UriUserInfo = 2,
        InputParameters = 99
    }
}
