using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferGUI.views;
using TwoStageFileTransferGUI.views.pages.shared;

namespace TwoStageFileTransferGUI.dto
{
    public class PageProperties
    {
        public string Name { get; set; }
        public Func<AppArgs, string> NextDecision { get; set; }
        public Type PageType { get; set; }
        public IPageSettings PageSettings { get; set; }

        public IPageApp CreateNextPageInstance()
        {
            return (IPageApp)Activator.CreateInstance(PageType);
        }
    }
}
