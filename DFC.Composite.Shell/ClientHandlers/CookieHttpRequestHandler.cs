using DFC.Composite.Shell.Services.PrefixCreator;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    /// <summary>
    /// Copies cookies from the shell to a child app
    /// </summary>
    public class CookieHttpRequestHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPrefixCreator _prefixCreator;

        public CookieHttpRequestHandler(IHttpContextAccessor httpContextAccessor, IPrefixCreator prefixCreator)
        {
            _httpContextAccessor = httpContextAccessor;
            _prefixCreator = prefixCreator;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var prefix = _prefixCreator.Resolve(request.RequestUri);
            var headers = _httpContextAccessor.HttpContext.Request.Headers;
            foreach (var header in headers)
            {
                if (ShouldAddHeader(header.Key) && !request.Headers.Contains(header.Key))
                {
                    var headerValues = header.Value.First().Split(';');
                    var cookieValues = new List<string>();
                    foreach (var headerValue in headerValues)
                    {
                        var headerValueTrimmed = headerValue.Trim();
                        if (ShouldAddCookie(prefix, headerValueTrimmed))
                        {
                            var cookieKey = GetCookieKey(prefix, headerValueTrimmed);
                            var cookieValue = GetCookieValue(headerValueTrimmed);

                            cookieValue = Uri.UnescapeDataString(cookieValue);

                            cookieValue = $"{cookieKey}={cookieValue}";
                            cookieValues.Add(cookieValue);
                        }
                    }

                    if (cookieValues.Any())
                    {
                        request.Headers.Add(HeaderNames.Cookie, cookieValues);
                    }

                }
            }

            return base.SendAsync(request, cancellationToken);
        }

        private string GetCookieKey(string prefix, string value)
        {
            var result = value.Split('=').First();
            if (result.StartsWith(prefix))
            {
                result = result.Substring(prefix.Length);
            }
            return result;
        }

        private string GetCookieValue(string value)
        {
            var result = string.Empty;
            var startPosition = value.IndexOf("=");
            if (startPosition != -1)
            {
                result = value.Substring(startPosition + 1);
            }
            return result;
        }

        private bool ShouldAddHeader(string key)
        {
            return key == "Cookie";
        }

        private bool ShouldAddCookie(string prefix, string value)
        {
            var segment = value.Split('=').First();
            var result = segment.StartsWith(prefix);
            return result;
        }
    }
}
