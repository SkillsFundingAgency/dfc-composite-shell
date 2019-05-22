using DFC.Composite.Shell.Common;
using System;

namespace DFC.Composite.Shell.Models
{
    public class RegionModel
    {
        public string Path { get; set; }
        public PageRegion PageRegion { get; set; }
        public bool IsHealthy { get; set; }
        public string RegionEndpoint { get; set; }
        public string HeathCheckRequired { get; set; }
        public DateTime DateOfRegistration { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
