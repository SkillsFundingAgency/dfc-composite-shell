using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController defaultController;
        private readonly IVersionedFiles defaultVersionedFiles;
        private readonly IConfiguration defaultConfiguration;

        public HomeControllerTests()
        {
            defaultVersionedFiles = A.Fake<IVersionedFiles>();
            defaultConfiguration = A.Fake<IConfiguration>();

            defaultController = new HomeController(defaultVersionedFiles, defaultConfiguration);
        }

        [Fact]
        public void HomeControllerIndexActionReturnsSuccess()
        {
            var result = defaultController.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<PageViewModel>(viewResult.ViewData.Model);
            Assert.Equal("Home", model.PageTitle);
        }

        [Fact]
        public void HomeControllerAlertActionReturnsSuccess()
        {
            var result = defaultController.Alert(404);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<PageViewModel>(viewResult.ViewData.Model);
            Assert.Equal("Page not found", model.PageTitle);
        }

        [Fact]
        public void HomeControllerErrorActionReturnsSuccess()
        {
            var homeController = new HomeController(defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext { TraceIdentifier = "SomeIdentifier" },
                },
            };

            var result = homeController.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ErrorViewModel>(viewResult.ViewData.Model);
            Assert.Equal("SomeIdentifier", model.RequestId);
            homeController.Dispose();
        }
    }
}