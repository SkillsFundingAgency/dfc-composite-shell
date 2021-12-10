using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;

using FakeItEasy;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class RobotControllerTests
    {
        private const string DummyScheme = "dummyScheme";
        private const string DummyHost = "dummyHost";
        private const string DummySitemapUrl = "/dummysitemap.xml";

        private readonly RobotController defaultController;
        private readonly IShellRobotFileService defaultShellRobotFileService;
        private readonly IAppRegistryDataService defaultAppRegistryDataService;
        private readonly ILogger<RobotController> defaultLogger;
        private readonly IWebHostEnvironment defaultWebHostEnvironment;
        private readonly HttpContext defaultHttpContext;
        private readonly IUrlHelper defaultUrlHelper;
        private readonly IBearerTokenRetriever defaultTokenRetriever;
        private readonly IApplicationRobotService defaultApplicationRobotService;
        private readonly IBaseUrlService defaultBaseUrlService;

        public RobotControllerTests()
        {
            defaultAppRegistryDataService = A.Fake<IAppRegistryDataService>();
            defaultLogger = A.Fake<ILogger<RobotController>>();
            defaultWebHostEnvironment = A.Fake<IWebHostEnvironment>();
            defaultBaseUrlService = A.Fake<IBaseUrlService>();

            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    RobotsURL = new Uri("http://SomeRobotUrl.xyz", UriKind.Absolute),
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
            A.CallTo(() => defaultUrlHelper.Content(A<string>.Ignored)).Returns("DummyUrl");
            A.CallTo(() => defaultUrlHelper.RouteUrl(A<UrlRouteContext>.Ignored)).Returns(DummySitemapUrl);

            defaultTokenRetriever = A.Fake<IBearerTokenRetriever>();
            A.CallTo(() => defaultTokenRetriever.GetToken(A<HttpContext>.Ignored)).Returns("SomeToken");

            defaultApplicationRobotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => defaultApplicationRobotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns("RetrievedValue: SomeValue");

            defaultShellRobotFileService = A.Fake<IShellRobotFileService>();
            A.CallTo(() => defaultShellRobotFileService.GetStaticFileText(A<string>.Ignored)).Returns("{Insertion}");

            defaultController = new RobotController(defaultAppRegistryDataService, defaultLogger, defaultWebHostEnvironment, defaultTokenRetriever, defaultApplicationRobotService, defaultShellRobotFileService, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };
        }

        public static IEnumerable<object[]> SegmentsToSkip => new List<object[]>
        {
            new object[] { "User-agent" },
            new object[] { "Sitemap" },
        };

        [Fact]
        public async Task RobotsControllerReturnsSuccess()
        {
            var result = await defaultController.Robot();

            Assert.True(!string.IsNullOrWhiteSpace(result.Content) && result.ContentType == MediaTypeNames.Text.Plain);
        }

        [Fact]
        public async Task RobotsControllerWritesShellRobotsTextAtFirstPositionWhenFileExists()
        {
            const string SomeShellFileText = "SomeFileText";

            A.CallTo(() => defaultShellRobotFileService.GetStaticFileText(A<string>.Ignored)).Returns(SomeShellFileText);

            var result = await defaultController.Robot();
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(resultLines[0], SomeShellFileText);
        }

        [Fact]
        public async Task RobotsControllerWritesSitemapDataToLastLineOfRobotText()
        {
            var expectedResult = $"Sitemap: " + $"{DummyScheme}://{DummyHost}".ToLowerInvariant() + "/sitemap/dummysitemap.xml";

            var result = await defaultController.Robot();

            Assert.NotEmpty(result.Content);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(expectedResult, resultLines[1]);
        }

        [Theory]
        [MemberData(nameof(SegmentsToSkip))]
        public async Task RobotsControllerRemovesUserAgentSegmentFromRobotText(string segmentToSkip)
        {
            var applicationRobotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => applicationRobotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns($"{segmentToSkip}: Dummy text value");

            var robotController = new RobotController(defaultAppRegistryDataService, defaultLogger, defaultWebHostEnvironment, defaultTokenRetriever, applicationRobotService, defaultShellRobotFileService, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            var result = await robotController.Robot();
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.DoesNotContain(segmentToSkip, resultLines.ToList());
            robotController.Dispose();
        }

        [Fact]
        public async Task RobotsControllerReplacesApplicationBaseUrlWithShellUrl()
        {
            const string appBaseUrl = "http://appBaseUrl";

            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    RobotsURL = new Uri(appBaseUrl, UriKind.Absolute),
                    IsOnline = true,
                },
            };

            var shellAppRegistryDataService = A.Fake<IAppRegistryDataService>();

            A.CallTo(() => shellAppRegistryDataService.GetAppRegistrationModels()).Returns(appRegistrationModels);

            var robotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => robotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns($"RetrievedValue: {appBaseUrl}/test");

            var robotController = new RobotController(shellAppRegistryDataService, defaultLogger, defaultWebHostEnvironment, defaultTokenRetriever, robotService, defaultShellRobotFileService, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            var result = await robotController.Robot();
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.DoesNotContain("http://appBaseUrl", resultLines.ToList());
            robotController.Dispose();
        }
    }
}