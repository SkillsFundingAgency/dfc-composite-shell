using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ListPathsViewComponent : ViewComponent
    {
        private readonly ILogger<ListPathsViewComponent> logger;
        private readonly IPathDataService pathDataService;

        public ListPathsViewComponent(ILogger<ListPathsViewComponent> logger, IPathDataService pathDataService)
        {
            this.logger = logger;
            this.pathDataService = pathDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ListPathsViewModel();

            try
            {
                vm.Paths = await pathDataService.GetPaths().ConfigureAwait(false);
            }
            catch (BrokenCircuitException ex)
            {
                var errorString = $"{nameof(ListPathsViewComponent)}: BrokenCircuit: {ex.Message}";

                logger.LogError(ex, errorString);
            }
            catch (Exception ex)
            {
                var errorString = $"{nameof(ListPathsViewComponent)}: {ex.Message}";

                logger.LogError(ex, errorString);

                ModelState.AddModelError(string.Empty, errorString);
            }

            return View(vm);
        }
    }
}