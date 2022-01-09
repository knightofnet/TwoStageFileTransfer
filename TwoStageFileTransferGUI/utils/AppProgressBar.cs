using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TwoStageFileTransferCore.utils;

namespace TwoStageFileTransferGUI.utils
{
    public class AppProgressBar : IProgressTransfer
    {
        private readonly Action<double, string> _actionReport;


        public AppProgressBar(Action<double, string> actionReport)
        {
            _actionReport = actionReport;
        }
        public void Dispose()
        {
            _actionReport(100, "");
        }

        public void Report(double value, string text)
        {
            _actionReport(value, text);
        }

        public void Init()
        {
            _actionReport(0, "");
        }

        public void SecondaryReport(string text)
        {
            _actionReport(Double.NaN, text);
        }
    }
}
