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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Fact]
        public async Task CanGetMarkupAsync_WhenMainBodyRegionEndpoint_IsEmpty()
        {
            //Arrange
            var path1 = "path1";
            var pathModels = new List<PathModel>();
            pathModels.Add(new PathModel() { Path = path1, TopNavigationOrder = 1 });

            var bodyRegionEndPoint = "https://localhost/bodyRegionEndpoint/";
            var footerRegionEndPoint = "https://localhost/footerRegionEndpoint/";
            var regions = new List<RegionModel>();
            regions.Add(new RegionModel() { PageRegion = PageRegion.Body, RegionEndpoint = bodyRegionEndPoint });
            regions.Add(new RegionModel() { PageRegion = PageRegion.BodyFooter, RegionEndpoint = footerRegionEndPoint });

            var app = new ApplicationModel() { Path = new PathModel() { Path = path1 } };
            app.Regions = regions;

            var bodyRegionContent = "bodyRegionContent";
            var footerRegionContent = "footerRegionContent";
            _pathService.Setup(x => x.GetPaths()).ReturnsAsync(pathModels);
            _regionService.Setup(x => x.GetRegions(It.IsAny<string>())).ReturnsAsync(regions);
            _contentRetriever.Setup(x => x.GetContent(bodyRegionEndPoint)).ReturnsAsync(bodyRegionContent);
            _contentRetriever.Setup(x => x.GetContent(footerRegionEndPoint)).ReturnsAsync(footerRegionContent);
            _contentProcessor.Setup(x => x.Process(It.IsAny<string>())).Returns<string>(x => x);

            //Act
            var pageModel = new PageViewModel();
            _mapper.Map(app, pageModel);
            await _applicationService.GetMarkupAsync(path1, string.Empty, pageModel);

            //Assert
            pageModel.PageRegionContentModels.Should().HaveCount(regions.Count());
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value.Should().Be(bodyRegionContent);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value.Should().Be(footerRegionContent);
            _contentRetriever.Verify(x => x.GetContent(bodyRegionEndPoint), Times.Once);
            _contentRetriever.Verify(x => x.GetContent(footerRegionEndPoint), Times.Once);
        }

        [Fact]
        public async Task CanGetMarkupAsync_When_MainBodyRegionIsRelativeUrl()
        {
            //Arrange
            var path1 = "path1";
            var pathModels = new List<PathModel>();
            pathModels.Add(new PathModel() { Path = path1, TopNavigationOrder = 1 });

            var bodyRegionEndPoint = "https://localhost/bodyRegionEndpoint/";
            var footerRegionEndPoint = "https://localhost/footerRegionEndpoint/";
            var regions = new List<RegionModel>();
            regions.Add(new RegionModel() { PageRegion = PageRegion.Body, RegionEndpoint = bodyRegionEndPoint });
            regions.Add(new RegionModel() { PageRegion = PageRegion.BodyFooter, RegionEndpoint = footerRegionEndPoint });

            var app = new ApplicationModel() { Path = new PathModel() { Path = path1 } };
            app.Regions = regions;

            var bodyRelativeUrl = "course/edit/1";
            var bodyAbsoluteUrl = "https://localhost/course/edit/1";
            var bodyRegionContent = "bodyRegionContent";
            var footerRegionContent = "footerRegionContent";
            _pathService.Setup(x => x.GetPaths()).ReturnsAsync(pathModels);
            _regionService.Setup(x => x.GetRegions(It.IsAny<string>())).ReturnsAsync(regions);
            _contentRetriever.Setup(x => x.GetContent(bodyAbsoluteUrl)).ReturnsAsync(bodyRegionContent);
            _contentRetriever.Setup(x => x.GetContent(footerRegionEndPoint)).ReturnsAsync(footerRegionContent);
            _contentProcessor.Setup(x => x.Process(It.IsAny<string>())).Returns<string>(x => x);

            //Act
            var pageModel = new PageViewModel();
            _mapper.Map(app, pageModel);
            await _applicationService.GetMarkupAsync(path1, bodyRelativeUrl, pageModel);

            //Assert
            pageModel.PageRegionContentModels.Should().HaveCount(regions.Count());
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value.Should().Be(bodyRegionContent);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value.Should().Be(footerRegionContent);
            _contentRetriever.Verify(x => x.GetContent(bodyRegionEndPoint), Times.Never);
            _contentRetriever.Verify(x => x.GetContent(footerRegionEndPoint), Times.Once);
            _contentRetriever.Verify(x => x.GetContent(bodyAbsoluteUrl), Times.Once);
        }
    }
}
