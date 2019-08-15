using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationServiceTests
    {
        private readonly IApplicationService applicationService;
        private readonly IMapper<ApplicationModel, PageViewModel> mapper;
        private readonly Mock<IPathDataService> pathDataService;
        private readonly Mock<IRegionService> regionService;
        private readonly Mock<IContentRetriever> contentRetriever;
        private readonly Mock<IContentProcessorService> contentProcessor;

        public ApplicationServiceTests()
        {
            mapper = new ApplicationToPageModelMapper();

            pathDataService = new Mock<IPathDataService>();
            regionService = new Mock<IRegionService>();
            contentRetriever = new Mock<IContentRetriever>();
            contentProcessor = new Mock<IContentProcessorService>();

            applicationService = new ApplicationService(
                pathDataService.Object,
                regionService.Object,
                contentRetriever.Object,
                contentProcessor.Object);
        }

        [Fact(Skip = "This needs to be broken up into separate tests by class")]
        public async Task CanGetMarkupAsyncForOnlineApplicationWhenContentUrlIsEmpty()
        {
            const string RequestBaseUrl = "https://localhost";

            //paths
            var path = "path1";
            var pathModel = new PathModel() { Path = path, TopNavigationOrder = 1, IsOnline = true };

            //regions
            var bodyRegionEndPoint = $"{RequestBaseUrl}/bodyRegionEndpoint";
            var footerRegionEndPoint = $"{RequestBaseUrl}/footerRegionEndpoint";
            var bodyRegion = new RegionModel() { PageRegion = PageRegion.Body, RegionEndpoint = bodyRegionEndPoint, IsHealthy = true };
            var bodyFooterRegion = new RegionModel() { PageRegion = PageRegion.BodyFooter, RegionEndpoint = footerRegionEndPoint, IsHealthy = true };
            var regions = new List<RegionModel>
            {
                bodyRegion,
                bodyFooterRegion,
            };

            //app
            var app = new ApplicationModel() { Path = pathModel, Regions = regions };

            //mocks
            var bodyRegionContent = "bodyRegionContent";
            var bodyFooterRegionContent = "bodyfooterRegionContent";
            pathDataService.Setup(x => x.GetPath(path)).ReturnsAsync(pathModel);
            regionService.Setup(x => x.GetRegions(It.IsAny<string>())).ReturnsAsync(regions);
            contentRetriever.Setup(x => x.GetContent(bodyRegion.RegionEndpoint, bodyRegion, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(bodyRegionContent);
            contentRetriever.Setup(x => x.GetContent(bodyFooterRegion.RegionEndpoint, bodyFooterRegion, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(bodyFooterRegionContent);
            contentProcessor.Setup(x => x.Process(bodyRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);
            contentProcessor.Setup(x => x.Process(bodyFooterRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);

            //Act
            var pageModel = new PageViewModel();
            mapper.Map(app, pageModel);
            applicationService.RequestBaseUrl = RequestBaseUrl;
            await applicationService.GetMarkupAsync(app, "index", pageModel).ConfigureAwait(false);

            //Assert
            pageModel.PageRegionContentModels.Should().HaveCount(regions.Count());
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value.Should().Be(bodyRegionContent);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value.Should().Be(bodyFooterRegionContent);
            contentRetriever.Verify(x => x.GetContent(bodyRegion.RegionEndpoint, bodyRegion, true, RequestBaseUrl), Times.Once);
            contentRetriever.Verify(x => x.GetContent(bodyFooterRegion.RegionEndpoint, bodyFooterRegion, true, RequestBaseUrl), Times.Once);
        }

        [Fact(Skip = "This needs to be broken up into separate tests by class")]
        public async Task CanGetMarkupAsyncForOnlineApplicationWhenContentUrlIsRelativeUrl()
        {
            //path
            var path = "path1";
            var pathModel = new PathModel() { Path = path, TopNavigationOrder = 1, IsOnline = true };

            //regions
            var bodyRegionEndPoint = "https://localhost:2000/bodyRegionEndpoint/";
            var footerRegionEndPoint = "https://localhost:2001/footerRegionEndpoint/";
            var bodyRegion = new RegionModel() { PageRegion = PageRegion.Body, RegionEndpoint = bodyRegionEndPoint, IsHealthy = true };
            var bodyFooterRegion = new RegionModel() { PageRegion = PageRegion.BodyFooter, RegionEndpoint = footerRegionEndPoint, IsHealthy = true };
            var regions = new List<RegionModel>
            {
                bodyRegion,
                bodyFooterRegion,
            };

            //app
            var app = new ApplicationModel() { Path = pathModel, Regions = regions };
            const string RequestBaseUrl = "https://localhost:2000";
            var bodyRelativeUrl = "course/edit/1";
            var bodyAbsoluteUrl = $"{RequestBaseUrl}/{path}/{bodyRelativeUrl}";
            var bodyRegionContent = "bodyRegionContent";
            var footerRegionContent = "footerRegionContent";
            pathDataService.Setup(x => x.GetPath(path)).ReturnsAsync(pathModel);
            regionService.Setup(x => x.GetRegions(It.IsAny<string>())).ReturnsAsync(regions);
            contentRetriever.Setup(x => x.GetContent(bodyAbsoluteUrl, null, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(bodyRegionContent);
            contentRetriever.Setup(x => x.GetContent(footerRegionEndPoint, null, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(footerRegionContent);
            contentProcessor.Setup(x => x.Process(bodyRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);
            contentProcessor.Setup(x => x.Process(footerRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);

            //Act
            var pageModel = new PageViewModel();
            mapper.Map(app, pageModel);
            applicationService.RequestBaseUrl = RequestBaseUrl;
            await applicationService.GetMarkupAsync(app, bodyRelativeUrl, pageModel).ConfigureAwait(false);

            //Assert
            pageModel.PageRegionContentModels.Should().HaveCount(regions.Count());
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value.Should().Be(bodyRegionContent);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value.Should().Be(footerRegionContent);
            contentRetriever.Verify(x => x.GetContent(bodyRegion.RegionEndpoint, bodyRegion, true, RequestBaseUrl), Times.Never);
            contentRetriever.Verify(x => x.GetContent(bodyFooterRegion.RegionEndpoint, bodyFooterRegion, true, RequestBaseUrl), Times.Once);
            contentRetriever.Verify(x => x.GetContent(bodyAbsoluteUrl, null, true, RequestBaseUrl), Times.Once);
        }
    }
}