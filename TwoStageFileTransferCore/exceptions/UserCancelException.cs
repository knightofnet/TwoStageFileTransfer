using System;
using System.Collections.Generic;
using TwoStageFileTransferCore.dto;

namespace TwoStageFileTransferCore.exceptions
{
    public class UserCancelArgs
    {
        public ICollection<AppFileFtp> FileTransfered;

        public long TotalByteRead { get; set; }
        public long TotalBytesToRead { get; internal set; }
    }

    public class UserCancelException : Exception
    {
        private UserCancelArgs userCancelArgs;

        public UserCancelException(UserCancelArgs userCancelArgs)
        {
            this.userCancelArgs = userCancelArgs;
        }
    }
}