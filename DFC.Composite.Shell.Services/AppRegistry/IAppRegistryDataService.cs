using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public interface IAppRegistryDataService
    {
        Task<AppRegistrationModel> GetAppRegistrationModel(string path);

        Task<IEnumerable<AppRegistrationModel>> GetAppRegistrationModels();

        Task<AppRegistrationModel> GetShellAppRegistrationModel();

        Task SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy);

        Task SetAjaxRequestHealthState(string path, string name, bool isHealthy);
    }
}