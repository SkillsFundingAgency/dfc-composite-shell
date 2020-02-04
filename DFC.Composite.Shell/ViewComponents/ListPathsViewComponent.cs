using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewComponent : ViewComponent
    {
        private readonly IPathDataService pathDataService;

        public ListPathsViewComponent(IPathDataService pathDataService)
        {
            this.pathDataService = pathDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ListPathsViewModel();

            var paths = await pathDataService.GetPaths().ConfigureAwait(false);

            vm.Paths = paths.Where(w => !string.IsNullOrWhiteSpace(w.TopNavigationText));

            return View(vm);
        }
    }
}