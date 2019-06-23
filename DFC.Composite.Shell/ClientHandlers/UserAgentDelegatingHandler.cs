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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CorrelationIdDelegatingHandler> _logger;

        public UserAgentDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CorrelationIdDelegatingHandler> logger)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(HeaderNames.UserAgent) && _httpContextAccessor.HttpContext != null)
            {
                foreach (var item in _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent])
                {
                    _logger.LogInformation($"Setting UserAgent to {item}");
                    request.Headers.Add(HeaderNames.UserAgent, item);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
