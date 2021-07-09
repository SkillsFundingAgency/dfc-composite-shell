using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public interface IAppRegistryService
    {
        Task<AppRegistrationModel> GetAppRegistrationModel(string path);

        Task<IEnumerable<AppRegistrationModel>> GetAppRegistrationModels();

        Task<AppRegistrationModel> GetShellAppRegistrationModel();

        Task SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy);

        Task SetAjaxRequestHealthState(string path, string name, bool isHealthy);
    }
}