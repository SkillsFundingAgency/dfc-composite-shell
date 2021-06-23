using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DFC.Composite.Shell.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ApplicationController> logger;
        private readonly IConfiguration configuration;

        public ErrorController(ILogger<ApplicationController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionPathDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var enhancedHttpException = exceptionPathDetails?.Error as EnhancedHttpException;
            var statusCode = enhancedHttpException?.StatusCode ?? HttpStatusCode.InternalServerError;
            var path = exceptionPathDetails?.Path ?? "unknown";

            var errorString = $"{nameof(Error)}: HttpStatusCode: {(int)statusCode} Unhandled error for path:{path}: {enhancedHttpException?.Message}";

            logger.LogError(exceptionPathDetails?.Error, errorString);

            Response.StatusCode = (int)statusCode;

            var viewModel = new PageViewModelResponse()
            {
                LayoutName = null,
                PageTitle = "Error | National Careers Service",
                BrandingAssetsCdn = configuration.GetValue<string>(nameof(PageViewModelResponse.BrandingAssetsCdn)),
                ScriptIds = new GoogleScripts(),
            };

            configuration?.GetSection(nameof(GoogleScripts)).Bind(viewModel.ScriptIds);

            return View(viewModel);
        }
    }
}