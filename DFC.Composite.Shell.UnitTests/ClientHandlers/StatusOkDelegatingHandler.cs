using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Test.ClientHandlers
{
    /// <summary>
    /// A test handler that returns a Status code of 200.
    /// </summary>
    public class StatusOkDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
