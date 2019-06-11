using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModel
    {
        public string Path { get; set; }
        public string LayoutName { get; set; } = "_LayoutFullWidth";
        public string BrandingAssetsCdn { get; set; } 
        public string Branding { get; set; } = "ESFA";
        public string PageTitle { get; set; } = "Unknown Service";
        public string PhaseBannerHtml { get; set; } 

        public List<PageRegionContentModel> PageRegionContentModels { get; set; }

        public PageViewModel()
        {
            PageRegionContentModels = new List<PageRegionContentModel>();
        }
    }
}
