using DFC.Composite.Shell.Services.PathLocator;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    /// <summary>
    /// Copies cookies from the shell to a child app.
    /// </summary>
    public class CookieDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPathLocator pathLocator;

        public CookieDelegatingHandler(IHttpContextAccessor httpContextAccessor, IPathLocator pathLocator)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.pathLocator = pathLocator;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var prefix = pathLocator.GetPath();

            CopyHeaders(prefix, httpContextAccessor.HttpContext.Request.Headers, request?.Headers);
            CopyHeaders(prefix, httpContextAccessor.HttpContext.Items, request?.Headers);

            return base.SendAsync(request, cancellationToken);
        }

        private static string GetCookieKey(string prefix, string value)
        {
            var result = value.Split('=').First();
            if (result.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(prefix.Length);
            }

            return result;
        }

        private static string GetCookieValue(string value)
        {
            var result = string.Empty;
            var startPosition = value.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (startPosition != -1)
            {
                result = value.Substring(startPosition + 1);
            }

            return result;
        }

        private static bool ShouldAddHeader(string key)
        {
            return key == HeaderNames.Cookie;
        }

        private static bool ShouldAddHeader(string prefix, string key)
        {
            return key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldAddCookie(string prefix, string value)
        {
            var segment = value.Split('=').First();
            var result = segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            return result;
        }

        private void CopyHeaders(string prefix, IHeaderDictionary sourceHeaders, HttpRequestHeaders destinationHeaders)
        {
            foreach (var sourceHeader in sourceHeaders)
            {
                if (ShouldAddHeader(sourceHeader.Key) && !destinationHeaders.Contains(sourceHeader.Key))
                {
                    var sourceHeaderValues = sourceHeader.Value.First().Split(';');
                    var cookieValues = new List<string>();
                    foreach (var sourceHeaderValue in sourceHeaderValues)
                    {
                        var sourceHeaderValueTrimmed = sourceHeaderValue.Trim();
                        if (ShouldAddCookie(prefix, sourceHeaderValueTrimmed))
                        {
                            var cookieKey = GetCookieKey(prefix, sourceHeaderValueTrimmed);
                            var cookieValue = GetCookieValue(sourceHeaderValueTrimmed);

                            cookieValue = Uri.UnescapeDataString(cookieValue);

                            cookieValue = $"{cookieKey}={cookieValue}";
                            cookieValues.Add(cookieValue);
                        }
                    }

                    if (cookieValues.Any())
                    {
                        destinationHeaders.Add(HeaderNames.Cookie, cookieValues);
                    }
                }
            }
        }

        private void CopyHeaders(string prefix, IDictionary<object, object> sourceHeaders, HttpRequestHeaders destinationHeaders)
        {
            var cookieValues = new List<string>();

            foreach (var sourceHeader in sourceHeaders)
            {
                var key = sourceHeader.Key.ToString();
                var value = sourceHeader.Value.ToString();
                if (ShouldAddHeader(prefix, key) && !destinationHeaders.Contains(key))
                {
                    var cookieKey = key.Replace(prefix, string.Empty, StringComparison.OrdinalIgnoreCase);
                    var cookieValue = $"{cookieKey}={value}";
                    cookieValues.Add(cookieValue);
                }
            }

            if (cookieValues.Any())
            {
                destinationHeaders.Add(HeaderNames.Cookie, cookieValues);
            }
        }
    }
}