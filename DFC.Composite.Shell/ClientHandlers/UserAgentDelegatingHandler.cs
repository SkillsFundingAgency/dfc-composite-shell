using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class UserAgentDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<CorrelationIdDelegatingHandler> logger;

        public UserAgentDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CorrelationIdDelegatingHandler> logger)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null && (!request.Headers.Contains(HeaderNames.UserAgent) && httpContextAccessor.HttpContext != null))
            {
                foreach (var item in httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent])
                {
                    logger.LogInformation($"Setting UserAgent to {item}");
                    request.Headers.Add(HeaderNames.UserAgent, item);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}