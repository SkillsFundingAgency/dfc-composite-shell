using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public class AppRegistryDataService : IAppRegistryDataService
    {
        private readonly IAppRegistryService appRegistryService;
        private IEnumerable<AppRegistrationModel> appRegistrationModels;

        public AppRegistryDataService(IAppRegistryService appRegistryService)
        {
            this.appRegistryService = appRegistryService;
        }

        public async Task<IEnumerable<AppRegistrationModel>> GetAppRegistrationModels()
        {
            return appRegistrationModels ?? (appRegistrationModels = await appRegistryService.GetPaths().ConfigureAwait(false));
        }

        public async Task<AppRegistrationModel> GetAppRegistrationModel(string path)
        {
            var models = await GetAppRegistrationModels().ConfigureAwait(false);

            return models?.FirstOrDefault(f => f.Path.ToUpperInvariant() == path.ToUpperInvariant());
        }

        public async Task SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            var appRegistrationModel = await GetAppRegistrationModel(path).ConfigureAwait(false);
            var regionModel = appRegistrationModel?.Regions?.FirstOrDefault(f => f.PageRegion == pageRegion);

            if (regionModel != null)
            {
                regionModel.IsHealthy = isHealthy;

                await appRegistryService.SetRegionHealthState(path, pageRegion, isHealthy).ConfigureAwait(false);
            }
        }
    }
}