using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
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
        private readonly Mock<ILogger<ExternalApplicationController>> _logger;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IApplicationService> _applicationService;

        public ExternalApplicationControllerTests()
        {
            _logger = new Mock<ILogger<ExternalApplicationController>>();
            _configuration = new Mock<IConfiguration>();
            _applicationService = new Mock<IApplicationService>();

            _controller = new ExternalApplicationController(_logger.Object, _configuration.Object, _applicationService.Object);
        }

        [Fact]
        public async Task Should_RedirectToExternalUrl_WhenPathIsExternal()
        {
            var path = "path1";
            var externalUrl = "http://www.google.com";
            var app = new ApplicationModel() { Path = new PathModel() { ExternalURL = externalUrl, Path = path } };

            _applicationService.Setup(x => x.GetApplicationAsync(path)).ReturnsAsync(app);

            var response = await _controller.Action(path);
            response.Should().BeOfType<RedirectResult>();

            var typedResponse = response as RedirectResult;
            typedResponse.Url.Should().Be(externalUrl);
        }

        [Fact]
        public async Task Should_RedirectToAppicationController_WhenPathIsNotExternal()
        {
            var path = "path1";
            var app = new ApplicationModel() { Path = new PathModel() { Path = path } };

            _applicationService.Setup(x => x.GetApplicationAsync(path)).ReturnsAsync(app);

            var response = await _controller.Action(path);
            response.Should().BeOfType<RedirectToActionResult>();

            var typedResponse = response as RedirectToActionResult;
            typedResponse.ControllerName.Should().Be("application");
            typedResponse.ActionName.Should().Be("action");
            typedResponse.RouteValues.Keys.Should().HaveElementAt(0, "path");
            typedResponse.RouteValues.Values.Should().HaveElementAt(0, path);
        }
    }
}
