using DFC.Composite.Shell.Controllers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class ErrorControllerTests
    {
        private readonly ILogger<ApplicationController> fakeLogger;

        public ErrorControllerTests()
        {
            fakeLogger = A.Fake<ILogger<ApplicationController>>();
        }

        [Fact]
        public void ErrorControllerErrorActionReturnsSuccess()
        {
            // Arrange
            string expectedUrl = $"/{ApplicationController.AlertPathName}/500";
            var errorController = new ErrorController(fakeLogger)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            // Act
            var result = errorController.Error();

            // Assert
            var statusResult = Assert.IsType<RedirectResult>(result);

            statusResult.Url.Should().Be(expectedUrl);
            A.Equals(false, statusResult.Permanent);
            errorController.Dispose();
        }
    }
}