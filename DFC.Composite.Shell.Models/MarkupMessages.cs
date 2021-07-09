using DFC.Composite.Shell.Models.Enums;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class MarkupMessages
    {
        private const string ServiceUnavailableHtml = @"<div class=""govuk-width-container""><h3>Service Unavailable</h3></div>";

        public string AppOfflineHtml { get; set; } =
            @"<div class=""govuk-width-container""><div class=""content-container""><h1 class=""heading-xlarge"">Sorry, there is a problem with this service</h1><p>Please try again later.</p><p><a href=""/contact-us"">Contact us</a> if you want to speak to one of our careers advisers.</p></div></div>";

        public Dictionary<PageRegion, string> RegionOfflineHtml { get; set; } = new Dictionary<PageRegion, string>
        {
            { PageRegion.Head, null },
            { PageRegion.Breadcrumb, null },
            { PageRegion.BodyTop, ServiceUnavailableHtml },
            { PageRegion.Body, ServiceUnavailableHtml },
            { PageRegion.SidebarRight, ServiceUnavailableHtml },
            { PageRegion.SidebarLeft, ServiceUnavailableHtml },
            { PageRegion.BodyFooter, ServiceUnavailableHtml },
            { PageRegion.HeroBanner, ServiceUnavailableHtml },
       };

        public string GetRegionOfflineHtml(PageRegion pageRegion)
        {
            return RegionOfflineHtml.ContainsKey(pageRegion) ? RegionOfflineHtml[pageRegion] : null;
        }
    }
}
