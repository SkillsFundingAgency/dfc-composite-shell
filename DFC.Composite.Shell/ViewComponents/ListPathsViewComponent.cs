using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

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

            try
            {
                var paths = await _pathService.GetPaths();

                vm.Paths = paths.Where(w => w.IsOnline);
            }
            catch (BrokenCircuitException ex)
            {
                var errorString = $" ListPathsViewComponent BrokenCircuit: {ex.Message}";
                ModelState.AddModelError(string.Empty, errorString);
            }

            return View(vm);
        }
    }
}
