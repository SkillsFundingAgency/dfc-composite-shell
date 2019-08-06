using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.SitemapModels;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.BaseUrlService;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.TokenRetriever;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
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

        private readonly SitemapController controller;
        private readonly ILogger<SitemapController> logger;
        private readonly HttpContext fakeHttpContext;
        private readonly IUrlHelper fakeUrlHelper;
        private readonly IBearerTokenRetriever fakeTokenRetriever;
        private readonly IApplicationSitemapService fakeSitemapService;
        private readonly IBaseUrlService baseUrlService;

        public SitemapControllerTests()
        {
            var pathDataService = A.Fake<IPathDataService>();
            logger = A.Fake<ILogger<SitemapController>>();
            baseUrlService = A.Fake<IBaseUrlService>();

            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    SitemapURL = "http://SomeSitemapUrl.xyz",
                    IsOnline = true,
                },
            };

            A.CallTo(() => pathDataService.GetPaths()).Returns(pathModels);

            var user = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);

            fakeHttpContext = A.Fake<HttpContext>();
            fakeHttpContext.Request.Scheme = DummyScheme;
            fakeHttpContext.Request.Host = new HostString(DummyHost);

            var fakeIdentity = new GenericIdentity("User");
            var principal = new GenericPrincipal(fakeIdentity, null);

            A.CallTo(() => fakeHttpContext.User).Returns(principal);

            fakeUrlHelper = A.Fake<IUrlHelper>();
            A.CallTo(() => fakeUrlHelper.Action(A<UrlActionContext>.Ignored)).Returns(DummyHomeIndex);

            fakeTokenRetriever = A.Fake<IBearerTokenRetriever>();
            A.CallTo(() => fakeTokenRetriever.GetToken(A<HttpContext>.Ignored)).Returns("SomeToken");

            A.CallTo(() => baseUrlService.GetBaseUrl(A<HttpRequest>.Ignored, A<IUrlHelper>.Ignored))
                .Returns("http://SomeBaseUrl");

            fakeSitemapService = A.Fake<IApplicationSitemapService>();
            A.CallTo(() => fakeSitemapService.GetAsync(A<ApplicationSitemapModel>.Ignored))
                .Returns(Task.FromResult<IEnumerable<SitemapLocation>>(new List<SitemapLocation>()
                {
                    new SitemapLocation
                    {
                        Url = "http://Sitemap.xml",
                        Priority = 1,
                    },
                }));

            controller = new SitemapController(pathDataService, logger, fakeTokenRetriever, baseUrlService, fakeSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
            };
        }

        [Fact]
        public async Task SitemapControllerReturnsSuccess()
        {
            var result = await controller.Sitemap().ConfigureAwait(false);

            Assert.True(!string.IsNullOrEmpty(result.Content) && result.ContentType == MediaTypeNames.Application.Xml);
        }

        [Fact]
        public async Task SitemapControllerWritesShellSitemapPathsText()
        {
            var result = await controller.Sitemap().ConfigureAwait(false);

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

            var sitemapController = new SitemapController(erroroneousPathDataService, logger, fakeTokenRetriever, baseUrlService, fakeSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
            };

            await sitemapController.Sitemap().ConfigureAwait(false);

            A.CallTo(() => logger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
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

            var sitemapController = new SitemapController(shellPathDataService, logger, fakeTokenRetriever, fakeBaseUrlService, applicationSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
            };

            var result = await sitemapController.Sitemap().ConfigureAwait(false);
            Assert.DoesNotContain(appBaseUrl, result.Content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("http://SomeBaseUrl", result.Content, StringComparison.OrdinalIgnoreCase);

            sitemapController.Dispose();
        }
    }
}