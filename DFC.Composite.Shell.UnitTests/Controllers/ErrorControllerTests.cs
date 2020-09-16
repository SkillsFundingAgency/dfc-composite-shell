using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class ErrorControllerTests
    {
        private readonly ILogger<ApplicationController> fakeLogger;
        private readonly IVersionedFiles fakeVersionedFiles;
        private readonly IConfiguration fakeConfiguration;

        public ErrorControllerTests()
        {
            fakeLogger = A.Fake<ILogger<ApplicationController>>();
            fakeVersionedFiles = A.Fake<IVersionedFiles>();
            fakeConfiguration = A.Fake<IConfiguration>();
        }

        [Fact]
        public void ErrorControllerErrorActionReturnsSuccess()
        {
            // Arrange
            using var errorController = new ErrorController(fakeLogger, fakeVersionedFiles, fakeConfiguration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            // Act
            var result = errorController.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<PageViewModel>(viewResult.ViewData.Model);
            Assert.Contains("Error", model.PageTitle, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}