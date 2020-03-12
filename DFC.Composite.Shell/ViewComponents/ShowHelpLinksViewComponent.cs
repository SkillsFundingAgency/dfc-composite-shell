using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ShowHelpLinksViewComponent : ViewComponent
    {
        private readonly ILogger<ShowHelpLinksViewComponent> logger;
        private readonly IPathDataService pathDataService;

        public ShowHelpLinksViewComponent(ILogger<ShowHelpLinksViewComponent> logger, IPathDataService pathDataService)
        {
            this.logger = logger;
            this.pathDataService = pathDataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ShowHelpLinksViewModel { IsOnline = false };

            try
            {
                var helpPath = await pathDataService.GetPath("help").ConfigureAwait(false);

                if (helpPath != null)
                {
                    vm.IsOnline = helpPath.IsOnline;
                    vm.OfflineHtml = new HtmlString(helpPath.OfflineHtml);
                }
            }
            catch (BrokenCircuitException ex)
            {
                var errorString = $"{nameof(ShowHelpLinksViewComponent)}: BrokenCircuit: {ex.Message}";

                logger.LogError(ex, errorString);

                throw;
            }

            return View(vm);
        }
    }
}