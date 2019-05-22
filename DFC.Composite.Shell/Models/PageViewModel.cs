using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class PageViewModel
    {
        public string Path { get; set; }
        public string LayoutName { get; set; } = "_LayoutFullWidth";
        public string Branding { get; set; } = "ESFA";
        public string PageTitle { get; set; } = "Unknown Service";

        public List<PageRegionContentModel> PageRegionContentModels { get; set; }

        public PageViewModel()
        {
            PageRegionContentModels = new List<PageRegionContentModel>();
        }
    }
}
