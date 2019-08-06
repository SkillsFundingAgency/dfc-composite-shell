using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Services.PathLocator;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ClientHandlers
{
    public class CookieDelegatingHandlerTest
    {
        private CookieDelegatingHandler handler;

        private IHttpContextAccessor httpContextAccessor;
        private IPathLocator pathLocator;

        [Fact]
        public async Task CanCopyHeadersFromShellToChildApp()
        {
            //Arrange
            var path1 = "path1";
            var path2 = "path2";
            var requestUrl = $"https://someurl.com/{path1}";

            //Create fakes
            pathLocator = A.Fake<IPathLocator>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();

            //Fake calls
            A.CallTo(() => pathLocator.GetPath()).Returns(path1);

            //Set some headers on the incoming request
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.Add(HeaderNames.Cookie, $"{path1}v1=value1;{path1}v2=value2;{path2}v3=value3;{path2}v4=value4");

            //Create a get request that is used to send data to the child app
            var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            //Create handlers and set the inner handler
            handler = new CookieDelegatingHandler(httpContextAccessor, pathLocator);
            handler.InnerHandler = new StatusOkDelegatingHandler();

            //Act
            var invoker = new HttpMessageInvoker(handler);
            var result = await invoker.SendAsync(httpRequestChildMessage, new CancellationToken());

            //Check that the child app has the correct number of headers based on the incoming request
            Assert.Single(httpRequestChildMessage.Headers);

            //Check that the values that are sent back are correct
            var headerValue = httpRequestChildMessage.Headers.First().Value;
            Assert.Equal("v1=value1", headerValue.First());
            Assert.Equal("v2=value2", headerValue.Last());
        }
    }
}
