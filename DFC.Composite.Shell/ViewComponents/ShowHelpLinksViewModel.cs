using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ShowHelpLinksViewModel
    {
        public bool IsOnline { get; set; }

        public HtmlString OfflineHtml { get; set; }
        public List<FooterHelpLinksModel> HelpLinks { get; set; }
    }
}