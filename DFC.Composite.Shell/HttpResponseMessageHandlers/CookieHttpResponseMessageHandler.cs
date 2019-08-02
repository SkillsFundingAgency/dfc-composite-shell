using DFC.Composite.Shell.Services.PrefixCreator;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Http;

namespace DFC.Composite.Shell.HttpResponseMessageHandlers
{
    /// <summary>
    /// Copies headers from a HttpResponseMessage and adds the to the responses cookies collection
    /// (ie from a child app to the Shell)
    /// </summary>
    public class CookieHttpResponseMessageHandler : IHttpResponseMessageHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPrefixCreator _prefixCreator;

        public CookieHttpResponseMessageHandler(IHttpContextAccessor httpContextAccessor, IPrefixCreator prefixCreator)
        {
            _httpContextAccessor = httpContextAccessor;
            _prefixCreator = prefixCreator;
        }

        public void Process(HttpResponseMessage httpResponseMessage)
        {
            var prefix = _prefixCreator.Resolve(httpResponseMessage.RequestMessage.RequestUri);
            foreach (var header in httpResponseMessage.Headers)
            {
                var headers = _httpContextAccessor.HttpContext.Response.Headers;
                if (IncludeHeader(header.Key))
                {
                    foreach (var headerValue in header.Value)
                    {
                        var cookieKey = GetKey(prefix, headerValue);
                        var cookieValue = GetValue(headerValue);
                        _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieKey, cookieValue);
                        if (!_httpContextAccessor.HttpContext.Items.ContainsKey(cookieKey))
                        {
                            _httpContextAccessor.HttpContext.Items[cookieKey] = cookieValue;
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
