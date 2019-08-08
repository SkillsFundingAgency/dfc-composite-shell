using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class ExternalApplicationControllerTests
    {
        private const string Path = "path1";
        private readonly ExternalApplicationController defaultController;
        private readonly IApplicationService defaultApplicationService;

        public ExternalApplicationControllerTests()
        {
            var logger = A.Fake<ILogger<ExternalApplicationController>>();
            defaultApplicationService = A.Fake<IApplicationService>();

            defaultController = new ExternalApplicationController(logger, defaultApplicationService);
        }

        [Fact]
        public async Task ShouldRedirectToExternalUrlWhenPathIsExternal()
        {
            const string externalUrl = "http://www.google.com";
            var applicationModel = new ApplicationModel { Path = new PathModel { ExternalURL = externalUrl, Path = Path } };
            A.CallTo(() => defaultApplicationService.GetApplicationAsync(A<string>.Ignored)).Returns(applicationModel);

            var response = await defaultController.Action(Path).ConfigureAwait(false);

            var result = Assert.IsAssignableFrom<RedirectResult>(response);
            Assert.Equal(result?.Url, externalUrl);
        }

        [Fact]
        public async Task ShouldRedirectToAppicationControllerWhenPathIsNotExternal()
        {
            var applicationModel = new ApplicationModel { Path = new PathModel { Path = Path } };
            A.CallTo(() => defaultApplicationService.GetApplicationAsync(A<string>.Ignored)).Returns(applicationModel);

            var response = await defaultController.Action(Path).ConfigureAwait(false);

            Assert.IsType<RedirectToActionResult>(response);
            var result = Assert.IsAssignableFrom<RedirectToActionResult>(response);
            Assert.True(result.ControllerName.Equals("application", StringComparison.OrdinalIgnoreCase)
                && result.ActionName.Equals("action", StringComparison.OrdinalIgnoreCase)
                && result.RouteValues.Keys.First().Equals("path", StringComparison.OrdinalIgnoreCase)
                && result.RouteValues.Values.First().ToString().Equals(Path, StringComparison.OrdinalIgnoreCase));
        }
    }
}