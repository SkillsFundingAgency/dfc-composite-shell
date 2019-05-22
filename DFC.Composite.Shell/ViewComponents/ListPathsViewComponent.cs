using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewComponent : ViewComponent
    {
        private readonly IPathService _pathService;

        public ListPathsViewComponent(IPathService pathService)
        {
            _pathService = pathService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ListPathsViewModel();
            vm.Paths = await _pathService.GetPaths();
            await Task.CompletedTask;
            return View(vm);
        }
    }
}
