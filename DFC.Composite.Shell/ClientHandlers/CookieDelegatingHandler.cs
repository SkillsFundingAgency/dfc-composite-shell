using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.DataProtectionProviders;
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
        private readonly ICompositeDataProtectionDataProvider compositeDataProtectionDataProvider;

        public CookieDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            IPathLocator pathLocator,
            ICompositeDataProtectionDataProvider compositeDataProtectionDataProvider)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.pathLocator = pathLocator;
            this.compositeDataProtectionDataProvider = compositeDataProtectionDataProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var prefix = pathLocator.GetPath();

            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "pages";
            }

            CopyHeaders(prefix, httpContextAccessor.HttpContext.Request.Headers, request?.Headers);
            CopyHeaders(prefix, httpContextAccessor.HttpContext.Items, request?.Headers);
            AddTokenHeaderFromCookie(httpContextAccessor.HttpContext, request);

            return base.SendAsync(request, cancellationToken);
        }

        private static string GetCookieKey(string prefix, string value)
        {
            var result = value.Split('=').First();

            if (result.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                result = result[prefix.Length..];
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
            var segments = value.Split('=');
            var segment = segments.FirstOrDefault();
            var result = segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || segment == Constants.DfcSession;

            return result;
        }

        private string GetCookieValue(string key, string value)
        {
            var result = string.Empty;
            var startPosition = value.IndexOf("=", StringComparison.OrdinalIgnoreCase);

            if (startPosition != -1)
            {
                result = value[(startPosition + 1) ..];
                result = Uri.UnescapeDataString(result);
            }

            if (!string.IsNullOrWhiteSpace(result) && key == Constants.DfcSession)
            {
                result = compositeDataProtectionDataProvider.Unprotect(result);
            }

            return result;
        }

        private void CopyHeaders(string prefix, IHeaderDictionary sourceHeaders, HttpRequestHeaders destinationHeaders)
        {
            foreach (var sourceHeader in sourceHeaders)
            {
                if (!ShouldAddHeader(sourceHeader.Key) || destinationHeaders.Contains(sourceHeader.Key))
                {
                    continue;
                }

                var sourceHeaderValues = sourceHeader.Value.First().Split(';');
                var cookieValues = new List<string>();

                foreach (var sourceHeaderValue in sourceHeaderValues)
                {
                    var sourceHeaderValueTrimmed = sourceHeaderValue.Trim();

                    if (!ShouldAddCookie(prefix, sourceHeaderValueTrimmed))
                    {
                        continue;
                    }

                    var cookieKey = GetCookieKey(prefix, sourceHeaderValueTrimmed);
                    var cookieValue = GetCookieValue(cookieKey, sourceHeaderValueTrimmed);

                    cookieValue = $"{cookieKey}={cookieValue}";
                    cookieValues.Add(cookieValue);
                }

                if (cookieValues.Any())
                {
                    destinationHeaders.Add(HeaderNames.Cookie, cookieValues);
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

                if (!ShouldAddHeader(prefix, key) || destinationHeaders.Contains(key))
                {
                    continue;
                }

                var cookieKey = key.Replace(prefix, string.Empty, StringComparison.OrdinalIgnoreCase);
                var cookieValue = $"{cookieKey}={value}";
                cookieValues.Add(cookieValue);
            }

            if (cookieValues.Any())
            {
                destinationHeaders.Add(HeaderNames.Cookie, cookieValues);
            }
        }

        private void AddTokenHeaderFromCookie(HttpContext context, HttpRequestMessage message)
        {
            if (context?.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var token = context.User.Claims
                .FirstOrDefault(claim => "bearer".Equals(claim.Type, StringComparison.OrdinalIgnoreCase))?.Value;

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
