using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Test.ClientHandlers;

using FakeItEasy;

using Microsoft.AspNetCore.Http;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.UnitTests.ClientHandlers
{
    public class CompositeRequestDelegatingHandlerTests
    {
        [Fact]
        public async Task AddsDFSHeaderWhenSendingARequest()
        {
            //Arrange
            var requestUrl = $"https://someurl.com";
            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            var headerName = "X-Dfc-Composite-Request";
            var headerValue1 = requestUrl;
            httpContextAccessor.HttpContext.Request.Headers.Add(headerName, headerValue1);

            var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            using (var handler = new CompositeRequestDelegatingHandler())
            {
                handler.InnerHandler = new StatusOkDelegatingHandler();

                //Act
                var invoker = new HttpMessageInvoker(handler);
                await invoker.SendAsync(httpRequestChildMessage, CancellationToken.None);

                //Assert
                Assert.Single(httpRequestChildMessage.Headers);
                Assert.True(httpRequestChildMessage.Headers.Contains(headerName));
                httpRequestChildMessage.Dispose();
                invoker.Dispose();
            }
        }
    }
}
