using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Exceptions;
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
            var enhancedHttpException = exceptionPathDetails?.Error as HttpException;
            var statusCode = enhancedHttpException?.StatusCode ?? HttpStatusCode.InternalServerError;
            var path = exceptionPathDetails?.Path ?? "unknown";

            logger.LogError(
                exceptionPathDetails?.Error,
                "{error}: HttpStatusCode: {statusCode} Unhandled error for path:{path}: {message}",
                nameof(Error),
                (int)statusCode,
                path,
                enhancedHttpException?.Message);

            var viewModel = new PageViewModelResponse
            {
                LayoutName = null,
                PageTitle = "Error | National Careers Service",
                BrandingAssetsCdn = configuration.GetValue<string>(nameof(PageViewModelResponse.BrandingAssetsCdn)),
                ScriptIds = new GoogleScripts(),
            };

            configuration?.GetSection(nameof(GoogleScripts)).Bind(viewModel.ScriptIds);

            Response.StatusCode = (int)statusCode;
            return View(viewModel);
        }
    }
}
