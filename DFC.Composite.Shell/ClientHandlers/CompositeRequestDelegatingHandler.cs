using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CompositeRequestDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headerName = "X-Dfc-Composite-Request";
            var headerValue = request?.RequestUri?.AbsoluteUri;
            if (request != null && !request.Headers.Contains(headerName) && !string.IsNullOrWhiteSpace(headerValue))
            {
                request.Headers.Add(headerName, headerValue);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}