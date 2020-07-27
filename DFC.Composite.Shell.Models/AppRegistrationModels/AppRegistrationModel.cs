using System;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models.AppRegistrationModels
{
    public class AppRegistrationModel
    {
        public string Path { get; set; }

        public string TopNavigationText { get; set; }

        public int TopNavigationOrder { get; set; }

        public PageLayout Layout { get; set; }

        public bool IsOnline { get; set; }

        public string OfflineHtml { get; set; }

        public string PhaseBannerHtml { get; set; }

        public Uri SitemapURL { get; set; }

        public Uri ExternalURL { get; set; }

        public Uri RobotsURL { get; set; }

        public DateTime DateOfRegistration { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public List<RegionModel> Regions { get; set; }

        public List<string> Locations { get; set; }
    }
}
