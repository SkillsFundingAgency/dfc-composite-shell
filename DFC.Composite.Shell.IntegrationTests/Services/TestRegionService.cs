using DFC.Composite.Shell.Models;
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
            var regions = new List<RegionModel>
            {
                new RegionModel()
                {
                    HeathCheckRequired = false,
                    IsHealthy = true,
                    OfflineHTML = $"{path} region body is offline",
                    PageRegion = PageRegion.Body,
                    Path = path,
                    RegionEndpoint = $"http://www.{path}.com/{path}/body",
                },
                new RegionModel()
                {
                    HeathCheckRequired = false,
                    IsHealthy = true,
                    OfflineHTML = $"{path} region head is offline",
                    PageRegion = PageRegion.Head,
                    Path = path,
                    RegionEndpoint = $"http://www.{path}.com/{path}/head",
                },
                new RegionModel()
                {
                    HeathCheckRequired = false,
                    IsHealthy = true,
                    OfflineHTML = $"{path} region breadcrumb is offline",
                    PageRegion = PageRegion.Breadcrumb,
                    Path = path,
                    RegionEndpoint = $"http://www.{path}.com/{path}/breadcrumb",
                },
                new RegionModel()
                {
                    HeathCheckRequired = false,
                    IsHealthy = false,
                    OfflineHTML = $"{path} region bodyfooter is offline",
                    PageRegion = PageRegion.BodyFooter,
                    Path = path,
                    RegionEndpoint = $"http://www.{path}.com/{path}/bodyfooter",
                },
            };

            return await Task.FromResult(regions).ConfigureAwait(false);
        }

        public Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            throw new NotImplementedException();
        }
    }
}