using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CompositeRequestDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            const string CompositeRequestHeaderName = "X-Dfc-Composite-Request";
            var headerValue = request?.RequestUri?.AbsoluteUri;

            if (request?.Headers.Contains(CompositeRequestHeaderName) == false && !string.IsNullOrWhiteSpace(headerValue))
            {
                request.Headers.Add(CompositeRequestHeaderName, headerValue);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
