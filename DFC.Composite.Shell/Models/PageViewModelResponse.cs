using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModelResponse
    {
        public PageViewModelResponse()
        {
        }

        public IList<string> VersionedPathForCssScripts { get; set; }

        public IList<string> VersionedPathForJavaScripts { get; set; }

        public string VersionedPathForWebChatJs { get; set; }

        public bool WebchatEnabled { get; set; }

        public string Path { get; set; }

        public string LayoutName { get; set; } = "_LayoutFullWidth";

        public string BrandingAssetsCdn { get; set; }

        public string PageTitle { get; set; } = "Unknown Service";

        public HtmlString PhaseBannerHtml { get; set; }

        public HtmlString ContentHead { get; set; }

        public HtmlString ContentHeroBanner { get; set; }

        public HtmlString ContentBreadcrumb { get; set; }

        public HtmlString ContentBodyTop { get; set; }

        public HtmlString ContentBody { get; set; }

        public HtmlString ContentSidebarRight { get; set; }

        public HtmlString ContentSidebarLeft { get; set; }

        public HtmlString ContentBodyFooter { get; set; }
    }
}