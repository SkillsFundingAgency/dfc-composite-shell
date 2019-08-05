using DFC.Composite.Shell.Services.PathLocator;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Http;

namespace DFC.Composite.Shell.HttpResponseMessageHandlers
{
    /// <summary>
    /// Copies headers from a HttpResponseMessage and adds the to the responses cookies collection
    /// (ie from a child app to the Shell).
    /// </summary>
    public class CookieHttpResponseMessageHandler : IHttpResponseMessageHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPathLocator pathLocator;

        public CookieHttpResponseMessageHandler(IHttpContextAccessor httpContextAccessor, IPathLocator pathLocator)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.pathLocator = pathLocator;
        }

        public void Process(HttpResponseMessage httpResponseMessage)
        {
            var prefix = pathLocator.GetPath();
            foreach (var header in httpResponseMessage.Headers)
            {
                var headers = httpContextAccessor.HttpContext.Response.Headers;
                if (IncludeHeader(header.Key))
                {
                    foreach (var headerValue in header.Value)
                    {
                        var cookieKey = GetKey(prefix, headerValue);
                        var cookieValue = GetValue(headerValue);
                        httpContextAccessor.HttpContext.Response.Cookies.Append(cookieKey, cookieValue);
                        if (!httpContextAccessor.HttpContext.Items.ContainsKey(cookieKey))
                        {
                            httpContextAccessor.HttpContext.Items[cookieKey] = cookieValue;
                        }
                    }
                }
            }
        }

        private string GetKey(string prefix, string headerValue)
        {
            var cookieKey = headerValue.Split('=').FirstOrDefault();
            var result = $"{prefix}{cookieKey}";
            return result;
        }

        private string GetValue(string headerValue)
        {
            var result = headerValue.Split(';').First().Split('=').LastOrDefault();
            return result;
        }

        private bool IncludeHeader(string key)
        {
            return key == HeaderNames.SetCookie;
        }
    }
}