using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransferCore.constant;

namespace TwoStageFileTransferCore.exceptions
{
    public class CommonAppException : Exception
    {
        public CommonAppExceptReason ExceptReason { get; private set; }
        public CommonAppException(string message, CommonAppExceptReason reason = CommonAppExceptReason.UnknownError) : base(message)
        {
            ExceptReason = reason;
        }

        public CommonAppException(string message, Exception innerException, CommonAppExceptReason reason = CommonAppExceptReason.UnknownError) : base(message, innerException)
        {
            ExceptReason = reason;
        }
    }
}
