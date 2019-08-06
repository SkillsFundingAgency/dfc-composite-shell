using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.BaseUrlService;
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
        private readonly RobotController controller;
        private readonly IShellRobotFileService shellRobotFileService;
        private readonly IPathDataService pathDataService;
        private readonly ILogger<RobotController> logger;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly HttpContext fakeHttpContext;
        private readonly IUrlHelper fakeUrlHelper;
        private readonly IBearerTokenRetriever fakeTokenRetriever;
        private readonly IApplicationRobotService fakeRobotService;
        private readonly IBaseUrlService baseUrlService;

        public RobotControllerTests()
        {
            pathDataService = A.Fake<IPathDataService>();
            logger = A.Fake<ILogger<RobotController>>();
            hostingEnvironment = A.Fake<IHostingEnvironment>();
            baseUrlService = A.Fake<IBaseUrlService>();

            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    RobotsURL = "http://SomeRobotUrl.xyz",
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
            A.CallTo(() => fakeUrlHelper.Content(A<string>.Ignored)).Returns("DummyUrl");
            A.CallTo(() => fakeUrlHelper.RouteUrl(A<UrlRouteContext>.Ignored)).Returns(DummySitemapUrl);

            fakeTokenRetriever = A.Fake<IBearerTokenRetriever>();
            A.CallTo(() => fakeTokenRetriever.GetToken(A<HttpContext>.Ignored)).Returns("SomeToken");

            fakeRobotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => fakeRobotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns("RetrievedValue: SomeValue");

            shellRobotFileService = A.Fake<IShellRobotFileService>();

            controller = new RobotController(pathDataService, logger, hostingEnvironment, fakeTokenRetriever, fakeRobotService, shellRobotFileService, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
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
            var result = await controller.Robot().ConfigureAwait(false);

            Assert.True(!string.IsNullOrEmpty(result.Content) && result.ContentType == MediaTypeNames.Text.Plain);
        }

        [Fact]
        public async Task RobotsControllerWritesShellRobotsTextAtFirstPositionWhenFileExists()
        {
            const string SomeShellFileText = "SomeFileText";

            A.CallTo(() => shellRobotFileService.GetFileText(A<string>.Ignored)).Returns(SomeShellFileText);

            var result = await controller.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(resultLines[0], SomeShellFileText);
        }

        [Fact]
        public async Task RobotsControllerWritesSitemapDataToLastLineOfRobotText()
        {
            var expectedResult = $"Sitemap: {DummyScheme}://{DummyHost}{DummySitemapUrl}";

            var result = await controller.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(resultLines[1], expectedResult);
        }

        [Theory]
        [MemberData(nameof(SegmentsToSkip))]
        public async Task RobotsControllerRemovesUserAgentSegmentFromRobotText(string segmentToSkip)
        {
            var applicationRobotService = A.Fake<IApplicationRobotService>();
            A.CallTo(() => applicationRobotService.GetAsync(A<ApplicationRobotModel>.Ignored)).Returns($"{segmentToSkip}: Dummy text value");

            var robotController = new RobotController(pathDataService, logger, hostingEnvironment, fakeTokenRetriever, applicationRobotService, shellRobotFileService, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
            };

            var result = await robotController.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.DoesNotContain(segmentToSkip, resultLines.ToList());
            robotController.Dispose();
        }

        [Fact]
        public async Task RobotsControllerWhenErroneousChildAppUrlThenErrorWrittenToLogger()
        {
            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    RobotsURL = "NotAValidUrl",
                    IsOnline = true,
                },
            };

            var erroroneousPathDataService = A.Fake<IPathDataService>();

            A.CallTo(() => erroroneousPathDataService.GetPaths()).Returns(pathModels);

            var robotController = new RobotController(erroroneousPathDataService, logger, hostingEnvironment, fakeTokenRetriever, fakeRobotService, shellRobotFileService, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
            };

            await robotController.Robot().ConfigureAwait(false);

            A.CallTo(() => logger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
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

            var robotController = new RobotController(shellPathDataService, logger, hostingEnvironment, fakeTokenRetriever, robotService, shellRobotFileService, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
                Url = fakeUrlHelper,
            };

            var result = await robotController.Robot().ConfigureAwait(false);
            var resultLines = result.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.DoesNotContain("http://appBaseUrl", resultLines.ToList());
            robotController.Dispose();
        }
    }
}