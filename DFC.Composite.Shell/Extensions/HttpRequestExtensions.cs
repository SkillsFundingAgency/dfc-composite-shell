using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Composite.Shell.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HttpRequestExtensions
    {
        public static Uri GetBaseAddress(this HttpRequest request, IUrlHelper? urlHelper = null)
        {
            const string xOriginaldProto = "X-Original-Proto";
            const string xOriginalHost = "X-Original-Host";

            if (request != null)
            {
                request.Headers.TryGetValue(xOriginaldProto, out var xOriginalProtocolValue);

                if (string.IsNullOrWhiteSpace(xOriginalProtocolValue))
                {
                    xOriginalProtocolValue = request.Scheme ?? Uri.UriSchemeHttp;
                }

                request.Headers.TryGetValue(xOriginalHost, out var xOriginalHostValue);

                if (string.IsNullOrWhiteSpace(xOriginalHostValue))
                {
                    xOriginalHostValue = request.Host.Value;
                }

                return new Uri($"{xOriginalProtocolValue}://{xOriginalHostValue}{urlHelper?.Content("~")}");
            }

            return default;
        }

        public static bool IsAjax(this HttpRequest request)
        {
            var headerValue = request?.Headers["X-Requested-With"];
            return headerValue?.Equals("XMLHttpRequest") ?? false;
        }
    }
}