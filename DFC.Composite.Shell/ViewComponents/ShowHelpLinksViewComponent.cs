using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ShowHelpLinksViewComponent : ViewComponent
    {
        private readonly ILogger<ShowHelpLinksViewComponent> _logger;
        private readonly IPathService _pathService;
        private readonly List<FooterHelpLinksModel> _helpLinks;

        public ShowHelpLinksViewComponent(ILogger<ShowHelpLinksViewComponent> logger, IPathService pathService, List<FooterHelpLinksModel> helpLinks)
        {
            _logger = logger;
            _pathService = pathService;
            _helpLinks = helpLinks;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new ShowHelpLinksViewModel
            {
                IsOnline = false,
                HelpLinks = _helpLinks
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
                _logger.LogError(ex, $"{nameof(ShowHelpLinksViewComponent)}: BrokenCircuit: {ex.Message}");
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