using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Services.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class ExternalApplicationController : BaseController
    {
        private const string MainRenderViewName = "Application/RenderView";

        private readonly ILogger<ExternalApplicationController> _logger;
        private readonly IApplicationService _applicationService;

        public ExternalApplicationController(ILogger<ExternalApplicationController> logger, IConfiguration configuration, IApplicationService applicationService)
        : base(configuration)
        {
            _logger = logger;
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(string path)
        {
            var application = await _applicationService.GetApplicationAsync(path);

            if (application != null && !string.IsNullOrWhiteSpace(application.Path.ExternalURL))
            {
                return Redirect(application.Path.ExternalURL);
            }

            return RedirectToAction("action", "application", new { path });
        }
    }
}