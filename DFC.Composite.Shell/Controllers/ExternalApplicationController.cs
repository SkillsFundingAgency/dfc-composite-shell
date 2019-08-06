using DFC.Composite.Shell.Services.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class ExternalApplicationController : Controller
    {
        private readonly ILogger<ExternalApplicationController> logger;
        private readonly IApplicationService applicationService;

        public ExternalApplicationController(ILogger<ExternalApplicationController> logger, IApplicationService applicationService)
        {
            this.logger = logger;
            this.applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(string path)
        {
            logger.LogInformation($"{nameof(Action)}: Getting external response for: {path}");

            var application = await applicationService.GetApplicationAsync(path).ConfigureAwait(false);

            if (application != null && !string.IsNullOrWhiteSpace(application.Path.ExternalURL))
            {
                logger.LogInformation($"{nameof(Action)}: Redirecting to external for: {path}");

                return Redirect(application.Path.ExternalURL);
            }

            logger.LogError($"{nameof(Action)}: Error getting external response for: {path}");

            return RedirectToAction("action", "application", new { path });
        }
    }
}