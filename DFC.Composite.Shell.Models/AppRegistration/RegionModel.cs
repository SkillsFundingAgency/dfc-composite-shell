using DFC.Composite.Shell.Models.Enums;
using System;

namespace DFC.Composite.Shell.Models.AppRegistration
{
    public class RegionModel
    {
        public PageRegion PageRegion { get; set; }

        public bool IsHealthy { get; set; } = true;

        public string RegionEndpoint { get; set; }

        public bool HealthCheckRequired { get; set; }

        public string OfflineHtml { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}