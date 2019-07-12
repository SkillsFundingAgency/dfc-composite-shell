using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class ExternalApplicationControllerTests
    {
        private readonly ExternalApplicationController _controller;
        private readonly Mock<IApplicationService> _applicationService;
        private const string Path = "path1";

        public ExternalApplicationControllerTests()
        {
            _applicationService = new Mock<IApplicationService>();
            var logger = new Mock<ILogger<ExternalApplicationController>>();
            var configuration = new Mock<IConfiguration>();
            var versionedFiles = new Mock<IVersionedFiles>();

            _controller = new ExternalApplicationController(logger.Object, configuration.Object, _applicationService.Object, versionedFiles.Object);
        }

        [Fact]
        public async Task Should_RedirectToExternalUrl_WhenPathIsExternal()
        {
            const string externalUrl = "http://www.google.com";
            var app = new ApplicationModel { Path = new PathModel { ExternalURL = externalUrl, Path = Path } };

            _applicationService.Setup(x => x.GetApplicationAsync(Path)).ReturnsAsync(app);

            var response = await _controller.Action(Path);
            response.Should().BeOfType<RedirectResult>();

            var typedResponse = response as RedirectResult;
            typedResponse?.Url.Should().Be(externalUrl);
        }

        [Fact]
        public async Task Should_RedirectToAppicationController_WhenPathIsNotExternal()
        {
            var app = new ApplicationModel { Path = new PathModel { Path = Path } };

            _applicationService.Setup(x => x.GetApplicationAsync(Path)).ReturnsAsync(app);

            var response = await _controller.Action(Path);
            response.Should().BeOfType<RedirectToActionResult>();

            var typedResponse = response as RedirectToActionResult;
            typedResponse?.ControllerName.Should().Be("application");
            typedResponse?.ActionName.Should().Be("action");
            typedResponse?.RouteValues.Keys.Should().HaveElementAt(0, "path");
            typedResponse?.RouteValues.Values.Should().HaveElementAt(0, Path);
        }
    }
}