using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Services.Regions
{
    public class LocalRegionService : IRegionService
    {
        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var regions = new List<RegionModel>();
            var baseEndpoint = "https://localhost:44394/";

            //This defines the entry point for this path
            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.Body,
                RegionEndpoint = baseEndpoint + "Course"
            });

            /*
            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.Breadcrumb,
                RegionEndpoint = "Breadcrumb"
            });

            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.BodyTop,
                RegionEndpoint = "BodyTop"
            });

            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.Head,
                RegionEndpoint = "Head"
            });

            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.SidebarLeft,
                RegionEndpoint = "SidebarLeft"
            });

            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.SidebarRight,
                RegionEndpoint = "SidebarRight"
            });*/

            regions.Add(new RegionModel()
            {
                PageRegion = PageRegion.Footer,
                RegionEndpoint = baseEndpoint + "Content/Footer"
            });

            return await Task.FromResult(regions);
        }

        public Task MarkAsHealthy(string path, PageRegion region)
        {
            throw new NotImplementedException();
        }

        public Task MarkAsUnhealthy(string path, PageRegion region)
        {
            throw new NotImplementedException();
        }
    }
}
