using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
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
        private readonly Mock<IAppRegistryDataService> appRegistryDataService;

        public ShowHelpLinksViewComponentTests()
        {
            logger = new Mock<ILogger<ShowHelpLinksViewComponent>>();
            appRegistryDataService = new Mock<IAppRegistryDataService>();

            viewComponent = new ShowHelpLinksViewComponent(logger.Object, appRegistryDataService.Object);
        }

        [Fact]
        public async Task ReturnsPathModelDetailsWhenPathExists()
        {
            var appRegistrationModel = new AppRegistrationModel() { IsOnline = true, OfflineHtml = "OfflineHtml" };
            appRegistryDataService.Setup(x => x.GetAppRegistrationModel(It.IsAny<string>())).ReturnsAsync(appRegistrationModel);

            var result = await viewComponent.InvokeAsync();

            var viewComponentModel = result.ViewDataModelAs<ShowHelpLinksViewModel>();
            Assert.Equal(appRegistrationModel.IsOnline, viewComponentModel.IsOnline);
            Assert.Equal(appRegistrationModel.OfflineHtml, viewComponentModel.OfflineHtml.Value);
        }

        [Fact]
        public async Task ReturnsOfflineFalseWhenPathDoesNotExist()
        {
            var appRegistrationModel = new AppRegistrationModel() { IsOnline = false };
            appRegistryDataService.Setup(x => x.GetAppRegistrationModel(It.IsAny<string>())).ReturnsAsync(appRegistrationModel);

            var result = await viewComponent.InvokeAsync();

            var viewComponentModel = result.ViewDataModelAs<ShowHelpLinksViewModel>();
            Assert.Equal(appRegistrationModel.IsOnline, viewComponentModel.IsOnline);
            Assert.Equal(appRegistrationModel.OfflineHtml, viewComponentModel.OfflineHtml.Value);
        }
    }
}
