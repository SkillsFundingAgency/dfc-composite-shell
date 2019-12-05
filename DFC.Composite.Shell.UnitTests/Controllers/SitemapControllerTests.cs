using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.SitemapModels;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.TokenRetriever;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class SitemapControllerTests
    {
        private const string DummyScheme = "dummyScheme";
        private const string DummyHost = "dummyHost";
        private const string DummyHomeIndex = "/DummyHomeIndex";

        private readonly SitemapController defaultController;
        private readonly ILogger<SitemapController> defaultLogger;
        private readonly HttpContext defaultHttpContext;
        private readonly IUrlHelper defaultUrlHelper;
        private readonly IBearerTokenRetriever defaultTokenRetriever;
        private readonly IApplicationSitemapService defaultSitemapService;
        private readonly IBaseUrlService defaultBaseUrlService;
        private readonly IPathDataService defaultPathDataService;

        public SitemapControllerTests()
        {
            defaultPathDataService = A.Fake<IPathDataService>();
            defaultLogger = A.Fake<ILogger<SitemapController>>();
            defaultBaseUrlService = A.Fake<IBaseUrlService>();

            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    SitemapURL = "http://SomeSitemapUrl.xyz",
                    IsOnline = true,
                },
            };

            A.CallTo(() => defaultPathDataService.GetPaths()).Returns(pathModels);

            var user = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);

            defaultHttpContext = A.Fake<HttpContext>();
            defaultHttpContext.Request.Scheme = DummyScheme;
            defaultHttpContext.Request.Host = new HostString(DummyHost);

            var fakeIdentity = new GenericIdentity("User");
            var principal = new GenericPrincipal(fakeIdentity, null);

            A.CallTo(() => defaultHttpContext.User).Returns(principal);

            defaultUrlHelper = A.Fake<IUrlHelper>();
            A.CallTo(() => defaultUrlHelper.Action(A<UrlActionContext>.Ignored)).Returns(DummyHomeIndex);

            defaultTokenRetriever = A.Fake<IBearerTokenRetriever>();
            A.CallTo(() => defaultTokenRetriever.GetToken(A<HttpContext>.Ignored)).Returns("SomeToken");

            A.CallTo(() => defaultBaseUrlService.GetBaseUrl(A<HttpRequest>.Ignored, A<IUrlHelper>.Ignored))
                .Returns("http://SomeBaseUrl");

            defaultSitemapService = A.Fake<IApplicationSitemapService>();
            A.CallTo(() => defaultSitemapService.GetAsync(A<ApplicationSitemapModel>.Ignored))
                .Returns(Task.FromResult<IEnumerable<SitemapLocation>>(new List<SitemapLocation>()
                {
                    new SitemapLocation
                    {
                        Url = "http://Sitemap.xml",
                        Priority = 1,
                    },
                }));

            defaultController = new SitemapController(defaultPathDataService, defaultLogger, defaultTokenRetriever, defaultBaseUrlService, defaultSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };
        }

        [Fact]
        public async Task SitemapControllerReturnsSuccess()
        {
            var result = await defaultController.Sitemap().ConfigureAwait(false);

            Assert.True(!string.IsNullOrWhiteSpace(result.Content) && result.ContentType == MediaTypeNames.Application.Xml);
        }

        [Fact]
        public async Task SitemapControllerWritesShellSitemapPathsText()
        {
            var result = await defaultController.Sitemap().ConfigureAwait(false);

            Assert.Contains(DummyHomeIndex, result.Content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SitemapControllerWhenErroneousChildAppUrlThenErrorWrittenToLogger()
        {
            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    SitemapURL = "NotAValidUrl",
                    IsOnline = true,
                },
            };

            var erroroneousPathDataService = A.Fake<IPathDataService>();

            A.CallTo(() => erroroneousPathDataService.GetPaths()).Returns(pathModels);

            var sitemapController = new SitemapController(erroroneousPathDataService, defaultLogger, defaultTokenRetriever, defaultBaseUrlService, defaultSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            await sitemapController.Sitemap().ConfigureAwait(false);

            A.CallTo(() => defaultLogger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
            sitemapController.Dispose();
        }

        [Fact]
        public async Task SitemapControllerReplacesApplicationBaseUrlWithShellUrl()
        {
            const string appBaseUrl = "http://appBaseUrl";

            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    SitemapURL = appBaseUrl,
                    IsOnline = true,
                },
            };

            var shellPathDataService = A.Fake<IPathDataService>();

            A.CallTo(() => shellPathDataService.GetPaths()).Returns(pathModels);

            var applicationSitemapService = A.Fake<IApplicationSitemapService>();
            A.CallTo(() => applicationSitemapService.GetAsync(A<ApplicationSitemapModel>.Ignored))
                .Returns(Task.FromResult<IEnumerable<SitemapLocation>>(new List<SitemapLocation>()
                {
                    new SitemapLocation
                    {
                        Url = $"{appBaseUrl}/test",
                        Priority = 1,
                    },
                }));

            var fakeBaseUrlService = A.Fake<IBaseUrlService>();
            A.CallTo(() => fakeBaseUrlService.GetBaseUrl(A<HttpRequest>.Ignored, A<IUrlHelper>.Ignored))
                .Returns("http://SomeBaseUrl");

            var sitemapController = new SitemapController(shellPathDataService, defaultLogger, defaultTokenRetriever, fakeBaseUrlService, applicationSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            var result = await sitemapController.Sitemap().ConfigureAwait(false);
            Assert.DoesNotContain(appBaseUrl, result.Content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("http://SomeBaseUrl", result.Content, StringComparison.OrdinalIgnoreCase);

            sitemapController.Dispose();
        }

        [Fact]
        public async Task RobotsControllerWhenBrokenCircuitExceptionThrownItIsLogged()
        {
            var fakeApplicationService = A.Fake<IApplicationSitemapService>();
            A.CallTo(() => fakeApplicationService.GetAsync(A<ApplicationSitemapModel>.Ignored))
                .Throws<BrokenCircuitException>();

            var sitemapController = new SitemapController(defaultPathDataService, defaultLogger, defaultTokenRetriever, defaultBaseUrlService, fakeApplicationService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            await sitemapController.Sitemap().ConfigureAwait(false);

            A.CallTo(() => defaultLogger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
            sitemapController.Dispose();
        }
    }
}