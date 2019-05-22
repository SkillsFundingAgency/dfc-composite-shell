using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Regions
{
    public interface IRegionService
    {
        Task<IEnumerable<RegionModel>> GetRegions(string path);
        Task MarkAsHealthy(string path, PageRegion region);
        Task MarkAsUnhealthy(string path, PageRegion region);
    }
}
