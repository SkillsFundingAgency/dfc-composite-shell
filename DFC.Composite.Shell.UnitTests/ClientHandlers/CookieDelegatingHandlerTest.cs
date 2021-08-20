using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.DataProtectionProviders;
using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.UnitTests.ClientHandlers;

using FakeItEasy;

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
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
        private ICompositeDataProtectionDataProvider compositeDataProtectionDataProvider;

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
            compositeDataProtectionDataProvider = A.Fake<ICompositeDataProtectionDataProvider>();

            //Fake calls
            A.CallTo(() => pathLocator.GetPath()).Returns(path1);
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());

            //Set some headers on the incoming request
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.Add(HeaderNames.Cookie, $"{path1}v1=value1;{path1}v2=value2;{path2}v3=value3;{path2}v4=value4");

            //Create a get request that is used to send data to the child app
            var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            //Create handlers and set the inner handler
            handler = new CookieDelegatingHandler(httpContextAccessor, pathLocator, compositeDataProtectionDataProvider)
            {
                InnerHandler = new StatusOkDelegatingHandler(),
            };

            //Act
            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(httpRequestChildMessage, CancellationToken.None);

            //Check that the child app has the correct number of headers based on the incoming request
            Assert.Single(httpRequestChildMessage.Headers);

            //Check that the values that are sent back are correct
            var headerValue = httpRequestChildMessage.Headers.First().Value.ToList();
            Assert.Equal("v1=value1", headerValue.First());
            Assert.Equal("v2=value2", headerValue.Last());
            httpRequestChildMessage.Dispose();
            invoker.Dispose();
        }

        [Fact]
        public async Task WhenHeadersFromShellToChildAppAreCopiedDfcSessionValuesAreCopiedAsIs()
        {
            //Arrange
            var path1 = "path1";
            var path2 = "path2";
            var requestUrl = $"https://someurl.com/{path1}";

            //Create fakes
            pathLocator = A.Fake<IPathLocator>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();
            compositeDataProtectionDataProvider = A.Fake<ICompositeDataProtectionDataProvider>();

            //Fake calls
            A.CallTo(() => pathLocator.GetPath()).Returns(path1);
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());

            //Set some headers on the incoming request
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.Add(HeaderNames.Cookie, $"{Constants.DfcSession}=sessionId1;{path1}v1=value1;{path1}v2=value2;{path2}v3=value3;{path2}v4=value4");

            //Create a get request that is used to send data to the child app
            var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            //Create handlers and set the inner handler
            handler = new CookieDelegatingHandler(httpContextAccessor, pathLocator, compositeDataProtectionDataProvider)
            {
                InnerHandler = new StatusOkDelegatingHandler(),
            };

            //Act
            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(httpRequestChildMessage, CancellationToken.None);

            //Check that the child app has the correct number of headers based on the incoming request
            Assert.Single(httpRequestChildMessage.Headers);

            //Check that the values that are sent back are correct
            var headerValue = httpRequestChildMessage.Headers.First().Value.ToList();
            Assert.Equal($"{Constants.DfcSession}=sessionId1", headerValue.First());
            httpRequestChildMessage.Dispose();
            invoker.Dispose();
        }

        [Fact]
        public async Task WhenHeadersFromShellToChildAppAreCopiedDfcSessionValuesAreProtected()
        {
            //Arrange
            var suffix = "_abc";
            var path1 = "path1";
            var path2 = "path2";
            var requestUrl = $"https://someurl.com/{path1}";

            //Create fakes
            pathLocator = A.Fake<IPathLocator>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();
            compositeDataProtectionDataProvider = A.Fake<ICompositeDataProtectionDataProvider>();

            //Fake calls
            A.CallTo(() => pathLocator.GetPath()).Returns(path1);
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString() + suffix);
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString().Substring(0, x.Arguments.First().ToString().Length - suffix.Length));

            //Set some headers on the incoming request
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.Add(HeaderNames.Cookie, $"{Constants.DfcSession}=sessionId1{suffix};{path1}v1=value1;{path1}v2=value2;{path2}v3=value3;{path2}v4=value4");

            //Create a get request that is used to send data to the child app
            var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            //Create handlers and set the inner handler
            handler = new CookieDelegatingHandler(httpContextAccessor, pathLocator, compositeDataProtectionDataProvider)
            {
                InnerHandler = new StatusOkDelegatingHandler(),
            };

            //Act
            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(httpRequestChildMessage, CancellationToken.None);

            //Check that the child app has the correct number of headers based on the incoming request
            Assert.Single(httpRequestChildMessage.Headers);

            //Check that the values that are sent back are correct
            var headerValue = httpRequestChildMessage.Headers.First().Value.ToList();
            Assert.Equal($"{Constants.DfcSession}=sessionId1", headerValue.First());
            httpRequestChildMessage.Dispose();
            invoker.Dispose();
        }

        [Fact]
        public async Task WhenShellAuthenticatedPassOnToken()
        {
            //Arrange
            var path1 = "path1";
            var path2 = "path2";
            var requestUrl = $"https://someurl.com/{path1}";

            //Create fakes
            pathLocator = A.Fake<IPathLocator>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();
            compositeDataProtectionDataProvider = A.Fake<ICompositeDataProtectionDataProvider>();

            //Fake calls
            A.CallTo(() => pathLocator.GetPath()).Returns(path1);
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());

            //Set some headers on the incoming request
            httpContextAccessor.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("bearer", "test") }, "mock")) };
            httpContextAccessor.HttpContext.Request.Headers.Add(HeaderNames.Cookie, $"{Constants.DfcSession}=sessionId1;{path1}v1=value1;{path1}v2=value2;{path2}v3=value3;{path2}v4=value4");
            httpContextAccessor.HttpContext.Session = new MockHttpSession();

            //Create a get request that is used to send data to the child app
            var httpRequestChildMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            //Create handlers and set the inner handler
            handler = new CookieDelegatingHandler(httpContextAccessor, pathLocator, compositeDataProtectionDataProvider)
            {
                InnerHandler = new StatusOkDelegatingHandler(),
            };

            //Act
            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(httpRequestChildMessage, CancellationToken.None);

            //Check that the values that are sent back are correct
            var headerValue = httpRequestChildMessage.Headers.Authorization;
            Assert.Equal("test", headerValue.Parameter);
            httpRequestChildMessage.Dispose();
            invoker.Dispose();
        }
    }
}