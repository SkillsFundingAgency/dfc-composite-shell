using CorrelationId;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor correlationContextAccessor;
        private readonly ILogger<CorrelationIdDelegatingHandler> logger;

        public CorrelationIdDelegatingHandler(
            ICorrelationContextAccessor correlationContextAccessor,
            ILogger<CorrelationIdDelegatingHandler> logger)
        {
            this.correlationContextAccessor = correlationContextAccessor;
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.correlationContextAccessor.CorrelationContext != null)
            {
                if (request != null && !request.Headers.Contains(correlationContextAccessor.CorrelationContext.Header))
                {
                    request.Headers.Add(correlationContextAccessor.CorrelationContext.Header, correlationContextAccessor.CorrelationContext.CorrelationId);
                    logger.Log(LogLevel.Information, $"Added CorrelationID header with name {correlationContextAccessor.CorrelationContext.Header} and value {correlationContextAccessor.CorrelationContext.CorrelationId}");
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}