using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult NegotiateContentResult(this Controller controller, object viewModel, object dataModel = null)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (controller.Request.Headers.Keys.Contains(HeaderNames.Accept))
            {
                var acceptHeaders = controller.Request.Headers[HeaderNames.Accept].ToString().ToLowerInvariant().Split(';');

                foreach (var acceptHeader in acceptHeaders)
                {
                    var items = acceptHeader.Split(',');

                    if (items.Contains(MediaTypeNames.Application.Json))
                    {
                        return controller.Ok(dataModel ?? viewModel);
                    }

                    if (items.Contains(MediaTypeNames.Text.Html) || items.Contains("*/*"))
                    {
                        return controller.View(viewModel);
                    }
                }
            }

            return controller.StatusCode((int)HttpStatusCode.NotAcceptable);
        }
    }
}
