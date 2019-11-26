using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.ViewComponents;
using DFC.Composite.Shell.Views.Test.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class ShowHelpLinksViewComponentTests
    {
        private readonly ShowHelpLinksViewComponent viewComponent;
        private readonly Mock<ILogger<ShowHelpLinksViewComponent>> logger;
        private readonly Mock<IPathDataService> pathDataService;

        public ShowHelpLinksViewComponentTests()
        {
            logger = new Mock<ILogger<ShowHelpLinksViewComponent>>();
            pathDataService = new Mock<IPathDataService>();

            viewComponent = new ShowHelpLinksViewComponent(logger.Object, pathDataService.Object);
        }

        [Fact]
        public async Task ReturnsPathModelDetailsWhenPathExists()
        {
            var pathModel = new PathModel() { IsOnline = true, OfflineHtml = "OfflineHtml" };
            pathDataService.Setup(x => x.GetPath(It.IsAny<string>())).ReturnsAsync(pathModel);

            var result = await viewComponent.InvokeAsync();

            var viewComponentModel = result.ViewDataModelAs<ShowHelpLinksViewModel>();
            Assert.Equal(pathModel.IsOnline, viewComponentModel.IsOnline);
            Assert.Equal(pathModel.OfflineHtml, viewComponentModel.OfflineHtml.Value);
        }

        [Fact]
        public async Task ReturnsOfflineFalseWhenPathDoesNotExist()
        {
            var pathModel = new PathModel() { IsOnline = false };
            pathDataService.Setup(x => x.GetPath(It.IsAny<string>())).ReturnsAsync(pathModel);

            var result = await viewComponent.InvokeAsync();

            var viewComponentModel = result.ViewDataModelAs<ShowHelpLinksViewModel>();
            Assert.Equal(pathModel.IsOnline, viewComponentModel.IsOnline);
            Assert.Equal(pathModel.OfflineHtml, viewComponentModel.OfflineHtml.Value);
        }
    }
}
