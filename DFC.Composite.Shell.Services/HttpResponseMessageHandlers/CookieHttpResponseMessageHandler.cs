using DFC.Composite.Shell.Services.CookieParsers;
using DFC.Composite.Shell.Services.HeaderRenamer;
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
        private readonly ISetCookieParser setCookieParser;
        private readonly IHeaderRenamerService headerRenamerService;

        public CookieHttpResponseMessageHandler(
            IHttpContextAccessor httpContextAccessor,
            IPathLocator pathLocator,
            ISetCookieParser setCookieParser,
            IHeaderRenamerService headerRenamerService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.pathLocator = pathLocator;
            this.setCookieParser = setCookieParser;
            this.headerRenamerService = headerRenamerService;
        }

        public void Process(HttpResponseMessage httpResponseMessage)
        {
            foreach (var header in httpResponseMessage?.Headers.Where(x => x.Key == HeaderNames.SetCookie))
            {
                foreach (var headerValue in header.Value)
                {
                    var cookieSettings = setCookieParser.Parse(headerValue);
                    var prefix = headerRenamerService.Rename(cookieSettings.Key) ? pathLocator.GetPath() : string.Empty;
                    var cookieKeyWithPrefix = string.Concat(prefix, cookieSettings.Key);
                    httpContextAccessor.HttpContext.Response.Cookies.Append(cookieKeyWithPrefix, cookieSettings.Value, cookieSettings.CookieOptions);
                    if (!httpContextAccessor.HttpContext.Items.ContainsKey(cookieKeyWithPrefix))
                    {
                        httpContextAccessor.HttpContext.Items[cookieKeyWithPrefix] = cookieSettings.Value;
                    }
                }
            }
        }
    }
}