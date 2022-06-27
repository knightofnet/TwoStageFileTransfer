using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.views;

namespace TwoStageFileTransferGUI.dto
{
    public interface IPageSettings
    {
        string PageTitle { get; set; }
        string PageDescription { get; set; }
        Func<AppArgs, string, bool> TreatmentCanGoNext { get; set; }
    }
}
