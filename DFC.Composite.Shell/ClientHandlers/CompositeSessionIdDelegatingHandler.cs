using DFC.Composite.Shell.Middleware;
using Microsoft.AspNetCore.Http;
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

            if (request != null && !request.Headers.Contains(HeaderName) && !string.IsNullOrWhiteSpace(compositeSessionId))
            {
                request.Headers.Add(HeaderName, compositeSessionId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
