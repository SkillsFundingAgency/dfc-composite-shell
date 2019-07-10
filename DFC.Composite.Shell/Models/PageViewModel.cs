using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModel
    {
        public string Path { get; set; }
        public string LayoutName { get; set; } = "_LayoutFullWidth";
        public string BrandingAssetsCdn { get; set; } 
        public string PageTitle { get; set; } = "Unknown Service";
        public HtmlString PhaseBannerHtml { get; set; } 

        public List<PageRegionContentModel> PageRegionContentModels { get; set; }

        public PageViewModel()
        {
            PageRegionContentModels = new List<PageRegionContentModel>();
        }
    }
}
