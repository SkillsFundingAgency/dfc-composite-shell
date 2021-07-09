using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModel
    {
        public IList<string> VersionedPathForCssScripts { get; set; }

        public IList<string> VersionedPathForJavaScripts { get; set; }

        public string VersionedPathForWebChatJs { get; set; }

        public bool WebchatEnabled { get; set; }

        public string Path { get; set; }

        public string LayoutName { get; set; } = "_LayoutFullWidth";

        public string BrandingAssetsCdn { get; set; }

        public string PageTitle { get; set; } = "Unknown Service";

        public HtmlString PhaseBannerHtml { get; set; }

        public List<PageRegionContentModel> PageRegionContentModels { get; set; } = new List<PageRegionContentModel>();

        public GoogleScripts ScriptIds { get; set; }
    }
}
