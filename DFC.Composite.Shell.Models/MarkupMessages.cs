using System.Collections.Generic;

namespace DFC.Composite.Shell.Models
{
    public class MarkupMessages
    {
        public string AppOfflineHtml { get; set; } = "<div class=\"govuk-width-container\"><div class=\"content-container\"><h1 class=\"heading-xlarge\">Sorry, there is a problem with this service</h1><p>Please try again later.</p><p><a href=\"/contact-us\">Contact us</a> if you want to speak to one of our careers advisers.</p></div></div>";

        public Dictionary<PageRegion, string> RegionOfflineHtml { get; set; } = new Dictionary<PageRegion, string> {
            {
                PageRegion.Head, null
            },
            {
                PageRegion.Breadcrumb, null
            },
            {
                PageRegion.BodyTop, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>"
            },
            {
                PageRegion.Body, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>"
            },
            {
                PageRegion.SidebarRight, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>"
            },
            {
                PageRegion.SidebarLeft, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>"
            },
            {
                PageRegion.BodyFooter, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>"
            },
            {
                PageRegion.HeroBanner, "<div class=\"govuk-width-container\"><h3>Service Unavailable</h3></div>"
            },
       };

        public string GetRegionOfflineHtml(PageRegion pageRegion)
        {
            return RegionOfflineHtml.ContainsKey(pageRegion) ? RegionOfflineHtml[pageRegion] : null;
        }
    }
}
