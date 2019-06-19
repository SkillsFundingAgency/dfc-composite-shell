using CorrelationId;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly ILogger<CorrelationIdDelegatingHandler> _logger;

        public CorrelationIdDelegatingHandler(
            ICorrelationContextAccessor correlationContextAccessor,
            ILogger<CorrelationIdDelegatingHandler> logger)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _logger = logger;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_correlationContextAccessor.CorrelationContext != null)
            {
                if (!request.Headers.Contains(_correlationContextAccessor.CorrelationContext.Header))
                {
                    request.Headers.Add(_correlationContextAccessor.CorrelationContext.Header, _correlationContextAccessor.CorrelationContext.CorrelationId);
                    _logger.Log(LogLevel.Information, $"Added CorrelationID header with name {_correlationContextAccessor.CorrelationContext.Header} and value {_correlationContextAccessor.CorrelationContext.CorrelationId}");
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
