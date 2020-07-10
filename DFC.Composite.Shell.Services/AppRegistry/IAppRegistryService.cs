using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public interface IAppRegistryService
    {
        Task<IEnumerable<AppRegistrationModel>> GetPaths();

        Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy);
    }
}
