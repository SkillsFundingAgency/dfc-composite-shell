using DFC.Composite.Shell.Common;
using Microsoft.AspNetCore.Html;

namespace DFC.Composite.Shell.Models
{
    public class PageRegionContentModel
    {
        public PageRegion PageRegionType { get; set; }

        public HtmlString Content { get; set; }
    }
}
