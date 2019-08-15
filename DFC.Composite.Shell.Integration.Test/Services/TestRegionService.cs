using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class TestRegionService : IRegionService
    {
        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var regions = new List<RegionModel>();

            regions.Add(new RegionModel()
            {
                HeathCheckRequired = false,
                IsHealthy = true,
                OfflineHTML = $"{path} region body is offline",
                PageRegion = PageRegion.Body,
                Path = path,
                RegionEndpoint = $"http://www.{path}.com/{path}/body"
            });
            regions.Add(new RegionModel()
            {
                HeathCheckRequired = false,
                IsHealthy = true,
                OfflineHTML = $"{path} head body is offline",
                PageRegion = PageRegion.Head,
                Path = path,
                RegionEndpoint = $"http://www.{path}.com/{path}/head"
            });

            return await Task.FromResult(regions);
        }

        public Task MarkAsHealthyAsync(string path, PageRegion pageRegion)
        {
            throw new NotImplementedException();
        }

        public Task MarkAsUnhealthyAsync(string path, PageRegion pageRegion)
        {
            throw new NotImplementedException();
        }
    }
}
