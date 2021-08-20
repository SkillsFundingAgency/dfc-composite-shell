using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.SitemapModels;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.TokenRetriever;

using FakeItEasy;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        private readonly SitemapController defaultController;
        private readonly ILogger<SitemapController> defaultLogger;
        private readonly HttpContext defaultHttpContext;
        private readonly IUrlHelper defaultUrlHelper;
        private readonly IBearerTokenRetriever defaultTokenRetriever;
        private readonly IApplicationSitemapService defaultSitemapService;
        private readonly IBaseUrlService defaultBaseUrlService;
        private readonly IAppRegistryDataService defaultAppRegistryDataService;

        public SitemapControllerTests()
        {
            defaultAppRegistryDataService = A.Fake<IAppRegistryDataService>();
            defaultLogger = A.Fake<ILogger<SitemapController>>();
            defaultBaseUrlService = A.Fake<IBaseUrlService>();

            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    SitemapURL = new Uri("http://SomeSitemapUrl.xyz", UriKind.Absolute),
                    IsOnline = true,
                },
            };

            A.CallTo(() => defaultAppRegistryDataService.GetAppRegistrationModels()).Returns(appRegistrationModels);

            var user = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => user.Identity.IsAuthenticated).Returns(true);

            defaultHttpContext = A.Fake<HttpContext>();
            defaultHttpContext.Request.Scheme = DummyScheme;
            defaultHttpContext.Request.Host = new HostString(DummyHost);

            var fakeIdentity = new GenericIdentity("User");
            var principal = new GenericPrincipal(fakeIdentity, null);

            A.CallTo(() => defaultHttpContext.User).Returns(principal);

            defaultUrlHelper = A.Fake<IUrlHelper>();

            defaultTokenRetriever = A.Fake<IBearerTokenRetriever>();
            A.CallTo(() => defaultTokenRetriever.GetToken(A<HttpContext>.Ignored)).Returns("SomeToken");

            A.CallTo(() => defaultBaseUrlService.GetBaseUrl(A<HttpRequest>.Ignored, A<IUrlHelper>.Ignored)).Returns("http://SomeBaseUrl");

            defaultSitemapService = A.Fake<IApplicationSitemapService>();
            A.CallTo(() => defaultSitemapService.GetAsync(A<ApplicationSitemapModel>.Ignored))
                .Returns(Task.FromResult<IEnumerable<SitemapLocation>>(new List<SitemapLocation>()
                {
                    new SitemapLocation
                    {
                        Url = "http://Sitemap.xml",
                    },
                }));

            defaultController = new SitemapController(defaultAppRegistryDataService, defaultLogger, defaultTokenRetriever, defaultBaseUrlService, defaultSitemapService)
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
            var result = await defaultController.Sitemap();

            Assert.True(!string.IsNullOrWhiteSpace(result.Content) && result.ContentType == MediaTypeNames.Application.Xml);
        }

        [Fact]
        public async Task SitemapControllerReplacesApplicationBaseUrlWithShellUrl()
        {
            const string appBaseUrl = "http://appBaseUrl";

            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    SitemapURL = new Uri(appBaseUrl, UriKind.Absolute),
                    IsOnline = true,
                },
            };

            var shellAppRegistryDataService = A.Fake<IAppRegistryDataService>();

            A.CallTo(() => shellAppRegistryDataService.GetAppRegistrationModels()).Returns(appRegistrationModels);

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

            var sitemapController = new SitemapController(shellAppRegistryDataService, defaultLogger, defaultTokenRetriever, fakeBaseUrlService, applicationSitemapService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            var result = await sitemapController.Sitemap();
            Assert.DoesNotContain(appBaseUrl, result.Content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("http://SomeBaseUrl", result.Content, StringComparison.OrdinalIgnoreCase);

            sitemapController.Dispose();
        }
    }
}