using Microsoft.AspNetCore.Html;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ShowHelpLinksViewModel
    {
        public bool IsOnline { get; set; }

        public HtmlString OfflineHtml { get; set; }
    }
}
