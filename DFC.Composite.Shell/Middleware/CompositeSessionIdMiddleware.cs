using DFC.Composite.Shell.ClientHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Middleware
{
    public class CompositeSessionIdMiddleware
    {
        internal const string NcsSessionCookieName = "ncs_session_cookie";

        private readonly RequestDelegate next;

        public CompositeSessionIdMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            string compositeSessionId = httpContext?.Request.Cookies[NcsSessionCookieName];

            if (string.IsNullOrWhiteSpace(compositeSessionId))
            {
                compositeSessionId = Guid.NewGuid().ToString();
            }

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(28),
                Secure = true,
                SameSite = SameSiteMode.None,
            };

            httpContext?.Response.Cookies.Append(NcsSessionCookieName, compositeSessionId, cookieOptions);

            var tempData = tempDataDictionaryFactory?.GetTempData(httpContext);
            if (tempData != null)
            {
                tempData[CompositeSessionIdDelegatingHandler.HeaderName] = compositeSessionId;
            }

            await next(httpContext).ConfigureAwait(false);
        }
    }
}
