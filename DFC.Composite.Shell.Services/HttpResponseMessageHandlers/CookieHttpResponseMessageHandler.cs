using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.CookieParsers;
using DFC.Composite.Shell.Services.DataProtectionProviders;
using DFC.Composite.Shell.Services.HeaderCount;
using DFC.Composite.Shell.Services.HeaderRenamer;
using DFC.Composite.Shell.Services.PathLocator;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
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
        private readonly IHeaderCountService headerCountService;
        private readonly ICompositeDataProtectionDataProvider compositeDataProtectionDataProvider;

        public CookieHttpResponseMessageHandler(
            IHttpContextAccessor httpContextAccessor,
            IPathLocator pathLocator,
            ISetCookieParser setCookieParser,
            IHeaderRenamerService headerRenamerService,
            IHeaderCountService headerCountService,
            ICompositeDataProtectionDataProvider compositeDataProtectionDataProvider)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.pathLocator = pathLocator;
            this.setCookieParser = setCookieParser;
            this.headerRenamerService = headerRenamerService;
            this.headerCountService = headerCountService;
            this.compositeDataProtectionDataProvider = compositeDataProtectionDataProvider;
        }

        public void Process(HttpResponseMessage httpResponseMessage)
        {
            var headers = new Dictionary<string, int>();
            foreach (var header in httpResponseMessage?.Headers.Where(x => x.Key == HeaderNames.SetCookie))
            {
                foreach (var headerValue in header.Value)
                {
                    var cookieSettings = setCookieParser.Parse(headerValue);
                    var cookieKey = cookieSettings.Key;
                    var prefix = headerRenamerService.Rename(cookieKey) ? pathLocator.GetPath() : string.Empty;
                    var cookieKeyWithPrefix = string.Concat(prefix, cookieKey);
                    var allowedHeaderCount = headerCountService.Count(cookieKey);
                    var currentHeaderCount = GetHeaderCount(headers, cookieKey);
                    var cookieValue = cookieSettings.Value;
                    if (cookieSettings.Key == Constants.DfcSession)
                    {
                        cookieValue = compositeDataProtectionDataProvider.Protect(cookieValue);
                    }

                    if (currentHeaderCount < allowedHeaderCount)
                    {
                        RegisterHeader(headers, cookieKey);
                        httpContextAccessor.HttpContext.Response.Cookies.Append(cookieKeyWithPrefix, cookieValue, cookieSettings.CookieOptions);
                        if (!httpContextAccessor.HttpContext.Items.ContainsKey(cookieKeyWithPrefix))
                        {
                            httpContextAccessor.HttpContext.Items[cookieKeyWithPrefix] = cookieValue;
                        }
                    }
                }
            }
        }

        private void RegisterHeader(Dictionary<string, int> headers, string headerName)
        {
            if (!headers.ContainsKey(headerName))
            {
                headers.Add(headerName, 1);
            }
            else
            {
                headers[headerName] += 1;
            }
        }

        private int GetHeaderCount(Dictionary<string, int> headers, string headerName)
        {
            var headerCount = 0;
            if (headers.ContainsKey(headerName))
            {
                headerCount = headers[headerName];
            }

            return headerCount;
        }
    }
}