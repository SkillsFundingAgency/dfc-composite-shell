using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModel
    {
        public PageViewModel()
        {
            PageRegionContentModels = new List<PageRegionContentModel>();
        }

        public string VersionedPathForMainMinCss { get; set; }

        public string VersionedPathForGovukMinCss { get; set; }

        public string VersionedPathForAllIe8Css { get; set; }

        public IList<string> VersionedPathForJavaScripts { get; set; }

        public string VersionedPathForWebChatJs { get; set; }

        public bool WebchatEnabled { get; set; }

        public string Path { get; set; }

        public string LayoutName { get; set; } = "_LayoutFullWidth";

        public string BrandingAssetsCdn { get; set; }

        public string PageTitle { get; set; } = "Unknown Service";

        public HtmlString PhaseBannerHtml { get; set; }

        public List<PageRegionContentModel> PageRegionContentModels { get; set; }
    }
}