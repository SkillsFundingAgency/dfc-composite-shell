using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Polly.CircuitBreaker;
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
        private const string DummySitemapUrl = "/DummySitemap.xml";

        private readonly RobotController defaultController;
        private readonly IShellRobotFileService defaultShellRobotFileService;
        private readonly IPathDataService defaultPathDataService;
        private readonly ILogger<RobotController> defaultLogger;
        private readonly IHostingEnvironment defaultHostingEnvironment;
        private readonly HttpContext defaultHttpContext;
        private readonly IUrlHelper defaultUrlHelper;
        private readonly IBearerTokenRetriever defaultTokenRetriever;
        private readonly IApplicationRobotService defaultApplicationRobotService;
        private readonly IBaseUrlService defaultBaseUrlService;

        public RobotControllerTests()
        {
            defaultPathDataService = A.Fake<IPathDataService>();
            defaultLogger = A.Fake<ILogger<RobotController>>();
            defaultHostingEnvironment = A.Fake<IHostingEnvironment>();
            defaultBaseUrlService = A.Fake<IBaseUrlService>();

            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    RobotsURL = "http://SomeRobotUrl.xyz",
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
            A.CallTo(() => defaultUrlHelper.Content(A<string>.Ignored)).Returns("DummyUrl");
            A.CallTo(() => defaultUrlHelper.RouteUrl(A<UrlRouteContext>.Ignored)).Returns(DummySitemapUrl);

            defaultTokenRetriever = A.Fake<IBearerTokenRetriever>();
            A.CallTo(() => defaultTokenRetriever.GetToken(A<HttpContext>.Ignored)).Returns("SomeToken");

            defaultApplicationRobotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => defaultApplicationRobotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns("RetrievedValue: SomeValue");

            defaultShellRobotFileService = A.Fake<IShellRobotFileService>();

            defaultController = new RobotController(defaultPathDataService, defaultLogger, defaultHostingEnvironment, defaultTokenRetriever, defaultApplicationRobotService, defaultShellRobotFileService, defaultBaseUrlService)
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
            var result = await defaultController.Robot().ConfigureAwait(false);

            Assert.True(!string.IsNullOrWhiteSpace(result.Content) && result.ContentType == MediaTypeNames.Text.Plain);
        }

        [Fact]
        public async Task RobotsControllerWritesShellRobotsTextAtFirstPositionWhenFileExists()
        {
            const string SomeShellFileText = "SomeFileText";

            A.CallTo(() => defaultShellRobotFileService.GetFileText(A<string>.Ignored)).Returns(SomeShellFileText);

            var result = await defaultController.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(resultLines[0], SomeShellFileText);
        }

        [Fact]
        public async Task RobotsControllerWritesSitemapDataToLastLineOfRobotText()
        {
            var expectedResult = $"Sitemap: {DummyScheme}://{DummyHost}{DummySitemapUrl}";

            var result = await defaultController.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(resultLines[1], expectedResult);
        }

        [Theory]
        [MemberData(nameof(SegmentsToSkip))]
        public async Task RobotsControllerRemovesUserAgentSegmentFromRobotText(string segmentToSkip)
        {
            var applicationRobotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => applicationRobotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns($"{segmentToSkip}: Dummy text value");

            var robotController = new RobotController(defaultPathDataService, defaultLogger, defaultHostingEnvironment, defaultTokenRetriever, applicationRobotService, defaultShellRobotFileService, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            var result = await robotController.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.DoesNotContain(segmentToSkip, resultLines.ToList());
            robotController.Dispose();
        }

        [Fact]
        public async Task RobotsControllerReplacesApplicationBaseUrlWithShellUrl()
        {
            const string appBaseUrl = "http://appBaseUrl";

            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    RobotsURL = appBaseUrl,
                    IsOnline = true,
                },
            };

            var shellPathDataService = A.Fake<IPathDataService>();

            A.CallTo(() => shellPathDataService.GetPaths()).Returns(pathModels);

            var robotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => robotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns($"RetrievedValue: {appBaseUrl}/test");

            var robotController = new RobotController(shellPathDataService, defaultLogger, defaultHostingEnvironment, defaultTokenRetriever, robotService, defaultShellRobotFileService, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultHttpContext,
                },
                Url = defaultUrlHelper,
            };

            var result = await robotController.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.DoesNotContain("http://appBaseUrl", resultLines.ToList());
            robotController.Dispose();
        }
    }
}