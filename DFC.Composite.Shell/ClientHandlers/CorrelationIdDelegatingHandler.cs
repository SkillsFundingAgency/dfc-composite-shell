using CorrelationId;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IOptions<CorrelationIdOptions> _options;
        private readonly ILogger<CorrelationIdDelegatingHandler> _logger;

        public CorrelationIdDelegatingHandler(
            ICorrelationContextAccessor correlationContextAccessor,
            IOptions<CorrelationIdOptions> options,
            ILogger<CorrelationIdDelegatingHandler> logger)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _options = options;
            _logger = logger;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_correlationContextAccessor.CorrelationContext != null)
            {
                if (!request.Headers.Contains(_options.Value.Header))
                {
                    request.Headers.Add(_options.Value.Header, _correlationContextAccessor.CorrelationContext.CorrelationId);
                    _logger.Log(LogLevel.Information, $"Added CorrelationID: {_correlationContextAccessor.CorrelationContext.CorrelationId}");
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
