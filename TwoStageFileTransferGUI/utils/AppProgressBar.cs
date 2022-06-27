using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils;
using TwoStageFileTransferCore.utils.events;
using TwoStageFileTransferGUI.dto;

namespace TwoStageFileTransferGUI.utils
{
    public class AppProgressBar : IProgressTransfer
    {
        public event TsftFileCreated TsftFileCreated;

        public event Action<string, double, BckgerReportType> Progress;
        public Func<bool> CheckIsCanceled { get; set; }
 

        public AppProgressBar()
        {
       
        }

        

        public void Init()
        {
            OnProgress(String.Empty, 0, BckgerReportType.ProgressPbarText );
        }

        public void Dispose()
        {
            OnProgress( String.Empty, 100, BckgerReportType.ProgressPbarText);
        }



        public void OnTsftFileCreated(TsftFileCreatedArgs args)
        {
            TsftFileCreated?.Invoke(this, args);
        }


        public virtual void OnProgress(string text, double percent, BckgerReportType bckType)
        {
           
            Progress?.Invoke(text, percent * 100, bckType);
        }
    }
}
