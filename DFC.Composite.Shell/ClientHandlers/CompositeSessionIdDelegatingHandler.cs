using DFC.Composite.Shell.Middleware;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CompositeSessionIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CompositeSessionIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            const string SessionIdHeaderName = "x-dfc-composite-sessionid";
            var compositeSessionId = httpContextAccessor?.HttpContext?.Request?.Cookies[CompositeSessionIdMiddleware.NcsSessionCookieName];

            if (request?.Headers.Contains(SessionIdHeaderName) == false && !string.IsNullOrWhiteSpace(compositeSessionId))
            {
                request.Headers.Add(SessionIdHeaderName, compositeSessionId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
