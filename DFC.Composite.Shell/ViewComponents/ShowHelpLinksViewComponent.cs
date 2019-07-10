using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ShowHelpLinksViewComponent : ViewComponent
    {
        private readonly ILogger<ShowHelpLinksViewComponent> _logger;
        private readonly IPathService _pathService;

        public ShowHelpLinksViewComponent(ILogger<ShowHelpLinksViewComponent> logger, IPathService pathService)
        {
            _logger = logger;
            _pathService = pathService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ShowHelpLinksViewModel()
            {
                IsOnline = false
            };

            try
            {
                var helpPath = await _pathService.GetPath("help");

                if (helpPath != null)
                {
                    vm.IsOnline = helpPath.IsOnline;
                    vm.OfflineHtml = new HtmlString(helpPath.OfflineHtml);
                }
            }
            catch (BrokenCircuitException ex)
            {
                var errorString = $"{nameof(ShowHelpLinksViewComponent)}: BrokenCircuit: {ex.Message}";

                _logger.LogError(ex, errorString);
            }
            catch (Exception ex)
            {
                var errorString = $"{nameof(ShowHelpLinksViewComponent)}: {ex.Message}";

                _logger.LogError(ex, errorString);

                ModelState.AddModelError(string.Empty, errorString);
            }

            return View(vm);
        }
    }
}
