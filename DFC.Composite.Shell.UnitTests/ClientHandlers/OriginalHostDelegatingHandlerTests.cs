using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Services.DataProtectionProviders;
using DFC.Composite.Shell.Services.PathLocator;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ClientHandlers
{
    public class OriginalHostDelegatingHandlerTests
    {
        [Fact]
        public async Task CanCopyHeadersFromShellToChildApp()
        {
            //Arrange
            var requestUrl = $"https://someurl.com/path1";

            //Create fakes
            var httpContextAccessor = A.Fake<IHttpContextAccessor>();

            //Set some headers on the incoming request
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.Add("X-Forwarded-Proto", "expected-scheme");
            httpContextAccessor.HttpContext.Request.Headers.Add("X-Original-Host", "expected-host");

            //Create a get request that is used to send data to the child app
            using var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            //Create handlers and set the inner handler
            using var handler = new OriginalHostDelegatingHandler(httpContextAccessor, A.Fake<ILogger<OriginalHostDelegatingHandler>>())
            {
                InnerHandler = new StatusOkDelegatingHandler(),
            };

            //Act
            using var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(httpRequestChildMessage, CancellationToken.None).ConfigureAwait(false);

            //Check that the child app has the correct number of headers based on the incoming request
            Assert.Equal(2, httpRequestChildMessage.Headers.Count());

            //Check that the values that are sent back are correct
            var headerValue = httpRequestChildMessage.Headers.ToList();
            Assert.Equal("expected-scheme", headerValue.First().Value.First());
            Assert.Equal("expected-host", headerValue.Last().Value.First());
        }
    }
}