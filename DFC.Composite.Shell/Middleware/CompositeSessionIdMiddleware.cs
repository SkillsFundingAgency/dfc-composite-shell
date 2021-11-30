using Microsoft.AspNetCore.Http;

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

        public Task Invoke(HttpContext httpContext)
        {
            var sessionIdString = httpContext?.Request.Cookies[NcsSessionCookieName];

            if (string.IsNullOrWhiteSpace(sessionIdString))
            {
                sessionIdString = Guid.NewGuid().ToString();
            }

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(28),
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
            };

            httpContext?.Response.Cookies.Append(NcsSessionCookieName, sessionIdString, cookieOptions);

            return next(httpContext);
        }
    }
}
