using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
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

            try
            {
                vm.Paths = await _pathService.GetPaths();
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
