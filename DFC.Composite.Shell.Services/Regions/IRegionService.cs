using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Regions
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionModel>> GetRegions(string path);

        Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy);
    }
}