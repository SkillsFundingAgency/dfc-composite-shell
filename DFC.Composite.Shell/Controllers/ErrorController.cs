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
        private readonly IVersionedFiles versionedFiles;
        private readonly IConfiguration configuration;

        public ErrorController(ILogger<ApplicationController> logger, IVersionedFiles versionedFiles, IConfiguration configuration)
        {
            this.logger = logger;
            this.versionedFiles = versionedFiles;
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

            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);
            viewModel.LayoutName = $"{Constants.LayoutPrefix}{PageLayout.FullWidth}";
            viewModel.PageTitle = "Error | National Careers Service";
            return View(viewModel);
        }
    }
}