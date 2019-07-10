using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using FluentAssertions;
using Moq;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationServiceTests
    {
        private readonly IApplicationService _applicationService;
        private readonly IMapper<ApplicationModel, PageViewModel> _mapper;
        private readonly Mock<IPathService> _pathService;
        private readonly Mock<IRegionService> _regionService;
        private readonly Mock<IContentRetriever> _contentRetriever;
        private readonly Mock<IContentProcessor> _contentProcessor;

        public ApplicationServiceTests()
        {
            _mapper = new ApplicationToPageModelMapper();

            _pathService = new Mock<IPathService>();
            _regionService = new Mock<IRegionService>();
            _contentRetriever = new Mock<IContentRetriever>();
            _contentProcessor = new Mock<IContentProcessor>();

            _applicationService = new ApplicationService(
                _pathService.Object,
                _regionService.Object,
                _contentRetriever.Object,
                _contentProcessor.Object);
        }

        [Fact(Skip = "This needs to be broken up into separate tests by class")]
        public async Task CanGetMarkupAsync_ForOnlineApplication_When_ContentUrl_IsEmpty()
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
                bodyFooterRegion
            };

            //app
            var app = new ApplicationModel() { Path = pathModel, Regions = regions };

            //mocks
            var bodyRegionContent = "bodyRegionContent";
            var bodyFooterRegionContent = "bodyfooterRegionContent";
            _pathService.Setup(x => x.GetPath(path)).ReturnsAsync(pathModel);
            _regionService.Setup(x => x.GetRegions(It.IsAny<string>())).ReturnsAsync(regions);
            _contentRetriever.Setup(x => x.GetContent(bodyRegion.RegionEndpoint, bodyRegion, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(bodyRegionContent);
            _contentRetriever.Setup(x => x.GetContent(bodyFooterRegion.RegionEndpoint, bodyFooterRegion, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(bodyFooterRegionContent);
            _contentProcessor.Setup(x => x.Process(bodyRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);
            _contentProcessor.Setup(x => x.Process(bodyFooterRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);

            //Act
            var pageModel = new PageViewModel();
            _mapper.Map(app, pageModel);
            _applicationService.RequestBaseUrl = RequestBaseUrl;
            await _applicationService.GetMarkupAsync(app, "index", pageModel);

            //Assert
            pageModel.PageRegionContentModels.Should().HaveCount(regions.Count());
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value.Should().Be(bodyRegionContent);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value.Should().Be(bodyFooterRegionContent);
            _contentRetriever.Verify(x => x.GetContent(bodyRegion.RegionEndpoint, bodyRegion, true, RequestBaseUrl), Times.Once);
            _contentRetriever.Verify(x => x.GetContent(bodyFooterRegion.RegionEndpoint, bodyFooterRegion, true, RequestBaseUrl), Times.Once);
        }

        [Fact(Skip = "This needs to be broken up into separate tests by class")]
        public async Task CanGetMarkupAsync_ForOnlineApplication_When_ContentUrl_IsRelativeUrl()
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
                bodyFooterRegion
            };

            //app
            var app = new ApplicationModel() { Path = pathModel, Regions = regions };
            const string RequestBaseUrl = "https://localhost:2000";
            var bodyRelativeUrl = "course/edit/1";
            var bodyAbsoluteUrl = $"{RequestBaseUrl}/{path}/{bodyRelativeUrl}";
            var bodyRegionContent = "bodyRegionContent";
            var footerRegionContent = "footerRegionContent";
            _pathService.Setup(x => x.GetPath(path)).ReturnsAsync(pathModel);
            _regionService.Setup(x => x.GetRegions(It.IsAny<string>())).ReturnsAsync(regions);
            _contentRetriever.Setup(x => x.GetContent(bodyAbsoluteUrl, null, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(bodyRegionContent);
            _contentRetriever.Setup(x => x.GetContent(footerRegionEndPoint, null, It.IsAny<bool>(), RequestBaseUrl)).ReturnsAsync(footerRegionContent);
            _contentProcessor.Setup(x => x.Process(bodyRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);
            _contentProcessor.Setup(x => x.Process(footerRegionContent, It.IsAny<string>(), It.IsAny<string>())).Returns<string, string, string>((x, y, z) => x);

            //Act
            var pageModel = new PageViewModel();
            _mapper.Map(app, pageModel);
            _applicationService.RequestBaseUrl = RequestBaseUrl;
            await _applicationService.GetMarkupAsync(app, bodyRelativeUrl, pageModel);

            //Assert
            pageModel.PageRegionContentModels.Should().HaveCount(regions.Count());
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value.Should().Be(bodyRegionContent);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value.Should().Be(footerRegionContent);
            _contentRetriever.Verify(x => x.GetContent(bodyRegion.RegionEndpoint, bodyRegion, true, RequestBaseUrl), Times.Never);
            _contentRetriever.Verify(x => x.GetContent(bodyFooterRegion.RegionEndpoint, bodyFooterRegion, true, RequestBaseUrl), Times.Once);
            _contentRetriever.Verify(x => x.GetContent(bodyAbsoluteUrl, null, true, RequestBaseUrl), Times.Once);
        }
    }
}
