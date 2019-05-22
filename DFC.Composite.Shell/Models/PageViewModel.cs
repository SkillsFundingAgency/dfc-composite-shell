using Microsoft.AspNetCore.Html;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModel
    {
        public string LayoutName { get; set; } = "_Layout";
        public string Branding { get; set; } = "ESFA";
        public string PageTitle { get; set; } = "Unknown Service";

        public HtmlString HeadMarkup { get; set; }
        public HtmlString BreadcrumbsMarkup { get; set; }
        public HtmlString BodyTopMarkup { get; set; }
        public HtmlString BodyMarkup { get; set; }
        public HtmlString SidebarRightMarkup { get; set; }
        public HtmlString SidebarLeftMarkup { get; set; }
        public HtmlString BodyFooterMarkup { get; set; }
    }
}
