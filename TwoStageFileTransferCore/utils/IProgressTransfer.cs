using System;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils.events;

namespace TwoStageFileTransferCore.utils
{
    public interface IProgressTransfer: IDisposable
    {
        event TsftFileCreated TsftFileCreated;

        event Action<string, double, BckgerReportType> Progress;

        Func<bool> CheckIsCanceled { get; set; }

        void Init();

        void OnTsftFileCreated(TsftFileCreatedArgs args);

        void OnProgress(string text, double percentDone=double.NaN, BckgerReportType bclReportType = BckgerReportType.ProgressTextOnly);
    }

    
}