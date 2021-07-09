using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public class AppRegistryService : IAppRegistryService
    {
        private readonly IAppRegistryRequestService appRegistryService;
        private IEnumerable<AppRegistrationModel> appRegistrationModels;

        public AppRegistryService(IAppRegistryRequestService appRegistryService)
        {
            this.appRegistryService = appRegistryService;
        }

        public async Task<IEnumerable<AppRegistrationModel>> GetAppRegistrationModels()
        {
            return appRegistrationModels ??= await appRegistryService.GetPaths();
        }

        public async Task<AppRegistrationModel> GetShellAppRegistrationModel()
        {
            return await GetAppRegistrationModel("shell");
        }

        public async Task<AppRegistrationModel> GetAppRegistrationModel(string path)
        {
            var models = await GetAppRegistrationModels();
            return models?.FirstOrDefault(model => model.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            var appRegistrationModel = await GetAppRegistrationModel(path);
            var regionModel = appRegistrationModel?.Regions?.FirstOrDefault(region => region.PageRegion == pageRegion);

            if (regionModel == null)
            {
                return;
            }

            regionModel.IsHealthy = isHealthy;
            await appRegistryService.SetRegionHealthState(path, pageRegion, isHealthy);
        }

        public async Task SetAjaxRequestHealthState(string path, string name, bool isHealthy)
        {
            var appRegistrationModel = await GetAppRegistrationModel(path);
            var ajaxRequestModel = appRegistrationModel?.AjaxRequests?
                .FirstOrDefault(ajaxRequest => ajaxRequest?.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);

            if (ajaxRequestModel == null)
            {
                return;
            }

            ajaxRequestModel.IsHealthy = isHealthy;
            await appRegistryService.SetAjaxRequestHealthState(path, name, isHealthy);
        }
    }
}