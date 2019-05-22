using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class PageModel
    {
        public string Path { get; set; }
        public string LayoutName { get; set; } 
        public string Branding { get; set; }
        public string PageTitle { get; set; }
            
        public List<PageRegionContentModel> PageRegionContentModels { get; set; }

        public PageModel()
        {
            PageRegionContentModels = new List<PageRegionContentModel>();
        }
    }
}
