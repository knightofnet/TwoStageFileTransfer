using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.constant;

namespace TwoStageFileTransfer.exceptions
{
    class AppException : Exception
    {

        public EnumExitCodes ExitCode { get; private set; }


        public AppException(string message, EnumExitCodes exitCode) : base(message)
        {
            ExitCode = exitCode;
        }

        public AppException(string message, Exception innerException, EnumExitCodes exitCode) : base(message, innerException)
        {
            ExitCode = exitCode;
        }





    }
}
