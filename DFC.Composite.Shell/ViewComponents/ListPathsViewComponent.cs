using DFC.Composite.Shell.Services.AppRegistry;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewComponent : ViewComponent
    {
        private readonly IAppRegistryService appRegistryDataService;

        public ListPathsViewComponent(IAppRegistryService appRegistryDataService)
        {
            this.appRegistryDataService = appRegistryDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewMdel = new ListPathsViewModel();

            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            viewMdel.AppRegistrationModels = appRegistrationModels.Where(model => !string.IsNullOrWhiteSpace(model.TopNavigationText));

            return View(viewMdel);
        }
    }
}
