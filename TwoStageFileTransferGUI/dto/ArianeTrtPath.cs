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
    public class ArianeTrtPath
    {
        public List<PageProperties> PathPages { get; private set; }

        public ArianeTrtPath()
        {
            PathPages = new List<PageProperties>();
        }


        public IPageApp NextPageInstance(string pageName, AppArgs appArgs)
        {
            PageProperties currentPageProps = PathPages.FirstOrDefault(r => r.Name.Equals(pageName));
            string nextPageName = currentPageProps.NextDecision(appArgs);
            PageProperties nextPageProps = PathPages.FirstOrDefault(r => r.Name.Equals(nextPageName));
            return (IPageApp)Activator.CreateInstance(nextPageProps.PageType);
        }

        public PageProperties NextPageProperties(string pageName, AppArgs appArgs)
        {
            PageProperties currentPageProps = PathPages.FirstOrDefault(r => r.Name.Equals(pageName));
            string nextPageName = currentPageProps.NextDecision(appArgs);
            return PathPages.FirstOrDefault(r => r.Name.Equals(nextPageName));
        }
    }
}
