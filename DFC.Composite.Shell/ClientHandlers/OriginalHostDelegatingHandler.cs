using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class OriginalHostDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<OriginalHostDelegatingHandler> logger;

        public OriginalHostDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<OriginalHostDelegatingHandler> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            const string xForwardedProtoHeader = "X-Forwarded-Proto";
            const string xOriginalHostHeader = "X-Original-Host";

            if (request == null)
            {
                return base.SendAsync(request, cancellationToken);
            }

            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return base.SendAsync(request, cancellationToken);
            }

            httpContext.Request.Headers.TryGetValue(xForwardedProtoHeader, out var xForwardedProtoValue);

            if (string.IsNullOrWhiteSpace(xForwardedProtoValue))
            {
                xForwardedProtoValue = httpContext.Request.Scheme;
            }

            request.Headers.Add(xForwardedProtoHeader, xForwardedProtoValue.ToString());
            logger.LogInformation(
                "Added Forwarded Proto header with name {xForwardedProtoHeader} and value {xForwardedProtoValue}",
                xForwardedProtoHeader,
                xForwardedProtoValue);

            httpContext.Request.Headers.TryGetValue(xOriginalHostHeader, out var xOriginalHostValue);

            if (string.IsNullOrWhiteSpace(xOriginalHostValue))
            {
                xOriginalHostValue = httpContext.Request.Host.Value;
            }

            request.Headers.Add(xOriginalHostHeader, xOriginalHostValue.ToString());
            logger.LogInformation(
                "Added Original Host header with name {xOriginalHostHeader} and value {xOriginalHostValue}",
                xOriginalHostHeader,
                xOriginalHostValue);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
