using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils.events;

namespace TwoStageFileTransferGUI.dto
{
    public class BckgerReportObj
    {
        public BckgerReportType Type { get; set; }

        public double PercentProgressed { get; set; }

        public string Text { get; set; }
        public object Object { get; set; }

        public BckgerReportObj(BckgerReportType type=BckgerReportType.ProgressPbarText, double percentProgressed = 0, string text = null)
        {
            Type = type;
            PercentProgressed = percentProgressed;
            Text = text;
        }
    }
}