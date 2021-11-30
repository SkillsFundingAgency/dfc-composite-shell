using DFC.Composite.Shell.Middleware;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CompositeSessionIdDelegatingHandler : DelegatingHandler
    {
        internal const string HeaderName = "x-dfc-composite-sessionid";

        private readonly IHttpContextAccessor httpContextAccessor;

        public CompositeSessionIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var compositeSessionId = httpContextAccessor?.HttpContext?.Request?.Cookies[CompositeSessionIdMiddleware.NcsSessionCookieName];
            // For intial requests or if a user has cleared cookies and no request cookie with a session id is present
            // we need to get it from the response cookie set by the CompositeSessionIdMiddleware else session storage will fail for these requests.
            if (string.IsNullOrWhiteSpace(compositeSessionId))
            {
                var newSessionCookie = httpContextAccessor?.HttpContext?.Response?.GetTypedHeaders().SetCookie?.FirstOrDefault(c => c.Name == CompositeSessionIdMiddleware.NcsSessionCookieName);
                if (newSessionCookie != null)
                {
                    compositeSessionId = newSessionCookie.Value.Value;
                }
            }

            if (request != null && !request.Headers.Contains(HeaderName) && !string.IsNullOrWhiteSpace(compositeSessionId))
            {
                request.Headers.Add(HeaderName, compositeSessionId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
