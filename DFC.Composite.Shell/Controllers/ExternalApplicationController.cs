using System.Threading.Tasks;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell.Controllers
{
    public class ExternalApplicationController : BaseController
    {
        private readonly ILogger<ExternalApplicationController> _logger;
        private readonly IApplicationService _applicationService;

        public ExternalApplicationController(ILogger<ExternalApplicationController> logger,
            IConfiguration configuration,
            IApplicationService applicationService,
            IVersionedFiles versionedFiles)
        : base(configuration, versionedFiles)
        {
            _logger = logger;
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(string path)
        {
            _logger.LogInformation($"{nameof(Action)}: Getting external response for: {path}");

            var application = await _applicationService.GetApplicationAsync(path);

            if (application != null && !string.IsNullOrWhiteSpace(application.Path.ExternalURL))
            {
                _logger.LogInformation($"{nameof(Action)}: Redirecting to external for: {path}");

                return Redirect(application.Path.ExternalURL);
            }

            _logger.LogError($"{nameof(Action)}: Error getting external response for: {path}");

            return RedirectToAction("action", "application", new { path });
        }
    }
}