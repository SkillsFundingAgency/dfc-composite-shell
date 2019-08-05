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
        private const string Path = "path1";
        private readonly ExternalApplicationController controller;
        private readonly Mock<IApplicationService> applicationService;

        public ExternalApplicationControllerTests()
        {
            applicationService = new Mock<IApplicationService>();
            var logger = new Mock<ILogger<ExternalApplicationController>>();
            var configuration = new Mock<IConfiguration>();
            var versionedFiles = new Mock<IVersionedFiles>();

            controller = new ExternalApplicationController(logger.Object, configuration.Object, applicationService.Object, versionedFiles.Object);
        }

        [Fact]
        public async Task ShouldRedirectToExternalUrlWhenPathIsExternal()
        {
            const string externalUrl = "http://www.google.com";
            var app = new ApplicationModel { Path = new PathModel { ExternalURL = externalUrl, Path = Path } };

            applicationService.Setup(x => x.GetApplicationAsync(Path)).ReturnsAsync(app);

            var response = await controller.Action(Path).ConfigureAwait(false);
            response.Should().BeOfType<RedirectResult>();

            var typedResponse = response as RedirectResult;
            typedResponse?.Url.Should().Be(externalUrl);
        }

        [Fact]
        public async Task ShouldRedirectToAppicationControllerWhenPathIsNotExternal()
        {
            var app = new ApplicationModel { Path = new PathModel { Path = Path } };

            applicationService.Setup(x => x.GetApplicationAsync(Path)).ReturnsAsync(app);

            var response = await controller.Action(Path).ConfigureAwait(false);
            response.Should().BeOfType<RedirectToActionResult>();

            var typedResponse = response as RedirectToActionResult;
            typedResponse?.ControllerName.Should().Be("application");
            typedResponse?.ActionName.Should().Be("action");
            typedResponse?.RouteValues.Keys.Should().HaveElementAt(0, "path");
            typedResponse?.RouteValues.Values.Should().HaveElementAt(0, Path);
        }
    }
}