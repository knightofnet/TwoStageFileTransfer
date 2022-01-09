using TwoStageFileTransferGUI.constant;

namespace TwoStageFileTransferGUI.dto
{
    public class BckgerReportObj
    {
        public BckgerReportType Type { get; set; }

        public int PercentProgressed { get; set; }

        public string Text { get; set; }

        public BckgerReportObj(BckgerReportType type=BckgerReportType.Classic, int percentProgressed = 0, string text = null)
        {
            Type = type;
            PercentProgressed = percentProgressed;
            Text = text;
        }
    }
}