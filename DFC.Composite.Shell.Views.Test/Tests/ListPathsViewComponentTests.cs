using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.ViewComponents;
using DFC.Composite.Shell.Views.Test.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class ListPathsViewComponentTests
    {
        private ListPathsViewComponent viewComponent;
        private Mock<ILogger<ListPathsViewComponent>> logger;
        private Mock<IPathDataService> pathDataService;

        public ListPathsViewComponentTests()
        {
            logger = new Mock<ILogger<ListPathsViewComponent>>();
            pathDataService = new Mock<IPathDataService>();

            viewComponent = new ListPathsViewComponent(logger.Object, pathDataService.Object);
        }

        [Fact]
        public async Task WhenInvokedReturnsPaths()
        {
            var pathModel1 = new PathModel() { Path = "path1", IsOnline = true, OfflineHtml = "OfflineHtml1" };
            var pathModel2 = new PathModel() { Path = "path2", IsOnline = true, OfflineHtml = "OfflineHtml2" };
            var paths = new List<PathModel>() { pathModel1, pathModel2 };
            pathDataService.Setup(x => x.GetPaths()).ReturnsAsync(paths);

            var result = await viewComponent.InvokeAsync();

            var viewComponentModel = result.ViewDataModelAs<ListPathsViewModel>();
            Assert.Equal(paths.Count, viewComponentModel.Paths.Count());
        }
    }
}
