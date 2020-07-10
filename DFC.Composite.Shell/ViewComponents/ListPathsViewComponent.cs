using DFC.Composite.Shell.Services.AppRegistry;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewComponent : ViewComponent
    {
        private readonly IAppRegistryDataService appRegistryDataService;

        public ListPathsViewComponent(IAppRegistryDataService appRegistryDataService)
        {
            this.appRegistryDataService = appRegistryDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ListPathsViewModel();

            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels().ConfigureAwait(false);

            vm.AppRegistrationModels = appRegistrationModels.Where(w => !string.IsNullOrWhiteSpace(w.TopNavigationText));

            return View(vm);
        }
    }
}