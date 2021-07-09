using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.ViewComponents;
using DFC.Composite.Shell.Views.Test.Extensions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class ListPathsViewComponentTests
    {
        private readonly ListPathsViewComponent viewComponent;
        private readonly Mock<IAppRegistryService> appRegistryDataService;

        public ListPathsViewComponentTests()
        {
            appRegistryDataService = new Mock<IAppRegistryService>();
            viewComponent = new ListPathsViewComponent(appRegistryDataService.Object);
        }

        [Fact]
        public async Task WhenInvokedReturnsPaths()
        {
            var appRegistrationModel1 = new AppRegistrationModel { Path = "path1", IsOnline = true, OfflineHtml = "OfflineHtml1", TopNavigationText = "Offline Html1" };
            var appRegistrationModel2 = new AppRegistrationModel { Path = "path2", IsOnline = true, OfflineHtml = "OfflineHtml2", TopNavigationText = "Offline Html2" };
            var appRegistrationModels = new List<AppRegistrationModel> { appRegistrationModel1, appRegistrationModel2 };
            appRegistryDataService.Setup(service => service.GetAppRegistrationModels()).ReturnsAsync(appRegistrationModels);

            var result = await viewComponent.InvokeAsync();

            var viewComponentModel = result.ViewDataModelAs<ListPathsViewModel>();
            Assert.Equal(appRegistrationModels.Count, viewComponentModel.AppRegistrationModels.Count());
        }
    }
}
