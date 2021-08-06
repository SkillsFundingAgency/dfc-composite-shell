using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
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
            const string xForwardedProto = "X-Forwarded-Proto";
            const string xOriginalHost = "X-Original-Host";

            if (request != null)
            {
                var httpContext = httpContextAccessor.HttpContext;

                if (httpContext?.Request == null)
                {
                    return base.SendAsync(request, cancellationToken);
                }

                var contextRequest = httpContext.Request;

                contextRequest.Headers.TryGetValue(xForwardedProto, out var xForwardedProtoValue);

                if (string.IsNullOrWhiteSpace(xForwardedProtoValue))
                {
                    xForwardedProtoValue = contextRequest.Scheme ?? Uri.UriSchemeHttp;
                }

                request.Headers.Add(xForwardedProto, xForwardedProtoValue.ToString());
                logger.Log(LogLevel.Information, $"Added Forwarded Proto header with name {xForwardedProto} and value {xForwardedProtoValue}");

                contextRequest.Headers.TryGetValue(xOriginalHost, out var xOriginalHostValue);

                if (string.IsNullOrWhiteSpace(xOriginalHostValue))
                {
                    xOriginalHostValue = contextRequest.Host.Value;
                }

                request.Headers.Add(xOriginalHost, xOriginalHostValue.ToString());
                logger.Log(LogLevel.Information, $"Added Original Host header with name {xOriginalHost} and value {xOriginalHostValue}");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}