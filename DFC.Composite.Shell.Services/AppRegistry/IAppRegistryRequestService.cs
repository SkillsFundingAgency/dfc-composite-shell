using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public interface IAppRegistryRequestService
    {
        Task<IEnumerable<AppRegistrationModel>> GetPaths();

        Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy);

        Task<bool> SetAjaxRequestHealthState(string path, string name, bool isHealthy);
    }
}
