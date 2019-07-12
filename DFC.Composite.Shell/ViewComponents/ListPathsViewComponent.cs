using System;
using System.Threading.Tasks;
using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewComponent : ViewComponent
    {
        private readonly ILogger<ListPathsViewComponent> _logger;
        private readonly IPathDataService _pathDataService;

        public ListPathsViewComponent(ILogger<ListPathsViewComponent> logger, IPathDataService pathDataService)
        {
            _logger = logger;
            _pathDataService = pathDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ListPathsViewModel();

            try
            {
                vm.Paths = await _pathDataService.GetPaths();
            }
            catch (BrokenCircuitException ex)
            {
                var errorString = $"{nameof(ListPathsViewComponent)}: BrokenCircuit: {ex.Message}";

                _logger.LogError(ex, errorString);
            }
            catch (Exception ex)
            {
                var errorString = $"{nameof(ListPathsViewComponent)}: {ex.Message}";

                _logger.LogError(ex, errorString);

                ModelState.AddModelError(string.Empty, errorString);
            }

            return View(vm);
        }
    }
}
