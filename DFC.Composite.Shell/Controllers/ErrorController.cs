using DFC.Composite.Shell.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DFC.Composite.Shell.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ApplicationController> logger;

        public ErrorController(ILogger<ApplicationController> logger)
        {
            this.logger = logger;
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionPathDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var enhancedHttpException = exceptionPathDetails?.Error as EnhancedHttpException;
            var statusCode = enhancedHttpException?.StatusCode ?? HttpStatusCode.InternalServerError;
            var path = exceptionPathDetails?.Path;

            var errorString = $"{nameof(Error)}: HttpStatusCode: {(int)statusCode} Unhandled error for:{path}: {enhancedHttpException?.Message}";

            logger.LogError(exceptionPathDetails?.Error, errorString);

            Response.StatusCode = (int)statusCode;

            return Redirect($"/{ApplicationController.AlertPathName}/{(int)statusCode}");
        }
    }
}