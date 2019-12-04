using Microsoft.AspNetCore.Html;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModelResponse
    {
        public PageViewModelResponse()
        {
        }

        public string VersionedPathForMainMinCss { get; set; }

        public string VersionedPathForGovukMinCss { get; set; }

        public string VersionedPathForAllIe8Css { get; set; }

        public string VersionedPathForJQueryBundleMinJs { get; set; }

        public string VersionedPathForAllMinJs { get; set; }

        public string VersionedPathForDfcDigitalMinJs { get; set; }

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