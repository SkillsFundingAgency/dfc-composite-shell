using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
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
    }
}