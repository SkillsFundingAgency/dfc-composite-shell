using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Services.Utilities;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationServiceTests
    {
        private const string RequestBaseUrl = "https://localhost";
        private const string Path = "path1";
        private const string HeadRegionContent = "headRegionContent";
        private const string BodyRegionContent = "bodyRegionContent";
        private const string BodyFooterRegionContent = "bodyfooterRegionContent";
        private const string OfflineHTML = "<p>Offline HTML</p>";
        private const string Article = "article";

        private readonly IApplicationService applicationService;
        private readonly IMapper<ApplicationModel, PageViewModel> mapper;
        private readonly IPathDataService pathDataService;
        private readonly IRegionService regionService;
        private readonly IContentRetriever contentRetriever;
        private readonly IContentProcessorService contentProcessor;
        private readonly PathModel defaultPathModel;
        private readonly RegionModel defaultHeadRegion;
        private readonly RegionModel defaultBodyRegion;
        private readonly RegionModel defaultBodyFooterRegion;
        private readonly List<RegionModel> defaultRegions;
        private readonly ApplicationModel defaultApplicationModel;
        private readonly ApplicationModel offlineApplicationModel;
        private readonly PageViewModel defaultPageViewModel;
        private readonly List<KeyValuePair<string, string>> defaultFormPostParams;
        private readonly ITaskHelper taskHelper;

        public ApplicationServiceTests()
        {
            mapper = new ApplicationToPageModelMapper();

            pathDataService = A.Fake<IPathDataService>();
            regionService = A.Fake<IRegionService>();
            contentRetriever = A.Fake<IContentRetriever>();
            contentProcessor = A.Fake<IContentProcessorService>();

            defaultPathModel = new PathModel { Path = Path, TopNavigationOrder = 1, IsOnline = true };

            var headRegionEndPoint = $"{RequestBaseUrl}/headRegionEndpoint";
            var bodyRegionEndPoint = $"{RequestBaseUrl}/bodyRegionEndpoint";
            var footerRegionEndPoint = $"{RequestBaseUrl}/footerRegionEndpoint";
            defaultHeadRegion = new RegionModel { PageRegion = PageRegion.Head, RegionEndpoint = headRegionEndPoint, IsHealthy = true, OfflineHTML = OfflineHTML };
            defaultBodyRegion = new RegionModel { PageRegion = PageRegion.Body, RegionEndpoint = bodyRegionEndPoint, IsHealthy = true, OfflineHTML = OfflineHTML };
            defaultBodyFooterRegion = new RegionModel { PageRegion = PageRegion.BodyFooter, RegionEndpoint = footerRegionEndPoint, IsHealthy = true, OfflineHTML = OfflineHTML };
            defaultRegions = new List<RegionModel>
            {
                defaultHeadRegion,
                defaultBodyRegion,
                defaultBodyFooterRegion,
            };

            defaultPageViewModel = new PageViewModel
            {
                PageRegionContentModels = new List<PageRegionContentModel>
                {
                    new PageRegionContentModel
                    {
                        PageRegionType = PageRegion.Body,
                    },
                },
            };

            defaultApplicationModel = new ApplicationModel { Path = defaultPathModel, Regions = defaultRegions };
            offlineApplicationModel = new ApplicationModel { Path = new PathModel { IsOnline = false, OfflineHtml = OfflineHTML } };

            A.CallTo(() => pathDataService.GetPath(Path)).Returns(defaultPathModel);
            A.CallTo(() => regionService.GetRegions(A<string>.Ignored)).Returns(defaultRegions);
            A.CallTo(() => contentRetriever.GetContent($"{defaultHeadRegion.RegionEndpoint}/index", defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl)).Returns(HeadRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyRegion.RegionEndpoint}/index", defaultBodyRegion, A<bool>.Ignored, RequestBaseUrl)).Returns(BodyRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}", defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl)).Returns(BodyFooterRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/index", defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl)).Returns(BodyFooterRegionContent);

            A.CallTo(() => contentProcessor.Process(HeadRegionContent, A<string>.Ignored, A<string>.Ignored)).Returns(HeadRegionContent);
            A.CallTo(() => contentProcessor.Process(BodyRegionContent, A<string>.Ignored, A<string>.Ignored)).Returns(BodyRegionContent);
            A.CallTo(() => contentProcessor.Process(BodyFooterRegionContent, A<string>.Ignored, A<string>.Ignored)).Returns(BodyFooterRegionContent);

            defaultFormPostParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("formParam1", "testvalue") };

            taskHelper = A.Fake<ITaskHelper>();
            A.CallTo(() => taskHelper.TaskCompletedSuccessfully(A<Task>.Ignored)).Returns(true);

            applicationService = new ApplicationService(pathDataService, regionService, contentRetriever, contentProcessor, taskHelper) { RequestBaseUrl = RequestBaseUrl };
        }

        public static IEnumerable<object[]> QueryStringParams => new List<object[]>
        {
            new object[] { string.Empty, $"{RequestBaseUrl}/headRegionEndpoint/{Article}" },
            new object[] { "invalid-query-string", $"{RequestBaseUrl}/headRegionEndpoint/{Article}" },
            new object[] { "?dummy-query-string=somedata", $"{RequestBaseUrl}/headRegionEndpoint/{Article}?dummy-query-string=somedata" },
        };

        [Fact]
        public async Task GetMarkupAsyncForOnlineApplication()
        {
            // Arrange
            var pageModel = new PageViewModel();
            mapper.Map(defaultApplicationModel, pageModel);

            //Act
            await applicationService.GetMarkupAsync(defaultApplicationModel, "index", pageModel, string.Empty).ConfigureAwait(false);

            //Assert
            Assert.Equal(defaultRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(HeadRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Head).Content.Value);
            Assert.Equal(BodyRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.GetContent($"{defaultHeadRegion.RegionEndpoint}/index", defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyRegion.RegionEndpoint}/index", defaultBodyRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/index", defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMarkupAsyncForOnlineApplicationWhenBodyRegionIsEmpty()
        {
            // Arrange
            var fakeBodyRegionEndPoint = string.Empty;
            var fakeBodyRegion = new RegionModel { PageRegion = PageRegion.Body, RegionEndpoint = fakeBodyRegionEndPoint, IsHealthy = true };
            var fakeRegions = new List<RegionModel> { defaultHeadRegion, fakeBodyRegion, defaultBodyFooterRegion };
            var fakeApplicationModel = new ApplicationModel { Path = defaultPathModel, Regions = fakeRegions };
            var pageModel = new PageViewModel();
            mapper.Map(fakeApplicationModel, pageModel);

            A.CallTo(() => regionService.GetRegions(A<string>.Ignored)).Returns(fakeRegions);
            A.CallTo(() => contentRetriever.GetContent($"{fakeBodyRegion.RegionEndpoint}/index", fakeBodyRegion, A<bool>.Ignored, RequestBaseUrl)).Returns(BodyRegionContent);

            //Act
            await applicationService.GetMarkupAsync(fakeApplicationModel, "index", pageModel, string.Empty).ConfigureAwait(false);

            //Assert
            Assert.Equal(fakeRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(HeadRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Head).Content.Value);
            Assert.Equal(string.Empty, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.GetContent($"{defaultHeadRegion.RegionEndpoint}/index", defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{fakeBodyRegion.RegionEndpoint}/index", fakeBodyRegion, A<bool>.Ignored, RequestBaseUrl)).MustNotHaveHappened();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/index", defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMarkupAsyncWhenApplicationIsOfflineThenOfflineHTMLIsReturned()
        {
            await applicationService.GetMarkupAsync(offlineApplicationModel, "index", defaultPageViewModel, string.Empty).ConfigureAwait(false);

            Assert.Equal(OfflineHTML, defaultPageViewModel.PageRegionContentModels.First().Content.ToString());
        }

        [Fact]
        public async Task GetMarkupAsyncWhenApplicationModelIsNullThenArgumentNullExceptionThrown()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await applicationService.GetMarkupAsync(null, "index", defaultPageViewModel, string.Empty).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetMarkupAsyncWhenPageViewModelIsNullThenArgumentNullExceptionThrown()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await applicationService.GetMarkupAsync(defaultApplicationModel, "index", null, string.Empty).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task PostMarkupAsyncWhenApplicationPathIsOfflineThenOfflineHTMLIsReturned()
        {
            await applicationService.PostMarkupAsync(offlineApplicationModel, "index", "article", defaultFormPostParams, defaultPageViewModel).ConfigureAwait(false);

            Assert.Equal(OfflineHTML, defaultPageViewModel.PageRegionContentModels.First().Content.ToString());
        }

        [Fact]
        public async Task PostMarkupAsyncForOnlineApplication()
        {
            // Arrange
            var footerAndBodyRegions = new List<RegionModel> { defaultBodyRegion, defaultBodyFooterRegion };
            var fakeApplicationModel = new ApplicationModel { Path = defaultPathModel, Regions = footerAndBodyRegions };
            var pageModel = new PageViewModel();
            mapper.Map(fakeApplicationModel, pageModel);

            A.CallTo(() => contentRetriever.PostContent($"{RequestBaseUrl}/{fakeApplicationModel.Path.Path}/{Article}", defaultBodyRegion, defaultFormPostParams, RequestBaseUrl)).Returns(BodyRegionContent);

            // Act
            await applicationService.PostMarkupAsync(fakeApplicationModel, "index", Article, defaultFormPostParams, pageModel).ConfigureAwait(false);

            //Assert
            Assert.Equal(footerAndBodyRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(BodyRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.PostContent($"{RequestBaseUrl}/{fakeApplicationModel.Path.Path}/{Article}", defaultBodyRegion, defaultFormPostParams, RequestBaseUrl)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}", defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PostMarkupAsyncForOnlineApplicationWhenBodyRegionEndpointIsEmptyThenContentNotPosted()
        {
            // Arrange
            var fakeBodyRegionEndpoint = string.Empty;

            var fakeBodyRegion = new RegionModel { PageRegion = PageRegion.Body, RegionEndpoint = fakeBodyRegionEndpoint, IsHealthy = true };
            var fakeRegions = new List<RegionModel> { fakeBodyRegion, defaultBodyFooterRegion };
            var fakeApplicationModel = new ApplicationModel { Path = defaultPathModel, Regions = fakeRegions };
            var pageModel = new PageViewModel();
            mapper.Map(fakeApplicationModel, pageModel);

            A.CallTo(() => contentRetriever.PostContent($"{RequestBaseUrl}/{fakeApplicationModel.Path.Path}/{Article}", fakeBodyRegion, defaultFormPostParams, RequestBaseUrl)).Returns(BodyRegionContent);

            // Act
            await applicationService.PostMarkupAsync(fakeApplicationModel, "index", Article, defaultFormPostParams, pageModel).ConfigureAwait(false);

            //Assert
            Assert.Equal(fakeRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(string.Empty, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.PostContent($"{RequestBaseUrl}/{fakeApplicationModel.Path.Path}/{Article}", fakeBodyRegion, defaultFormPostParams, RequestBaseUrl)).MustNotHaveHappened();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}", defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetApplicationAsyncReturnsEmptyApplicationModelWhenPathNotFound()
        {
            // Arrange
            var pathService = A.Fake<IPathDataService>();
            A.CallTo(() => pathService.GetPath(A<string>.Ignored)).Returns((PathModel)null);

            // Act
            var service = new ApplicationService(pathService, regionService, contentRetriever, contentProcessor, taskHelper);
            var result = await service.GetApplicationAsync(Path).ConfigureAwait(false);

            // Assert
            Assert.Null(result.RootUrl);
            Assert.Null(result.Path);
            Assert.Null(result.Regions);
        }

        [Fact]
        public async Task GetApplicationAsyncReturnsPathsAndRegionsAndRootUriWhenBodyRegionExists()
        {
            // Arrange
            var bodyAndFooterRegions = new List<RegionModel>
            {
                defaultBodyRegion,
                defaultBodyFooterRegion,
            };

            var fakeRegionService = A.Fake<IRegionService>();
            A.CallTo(() => fakeRegionService.GetRegions(defaultPathModel.Path)).Returns(bodyAndFooterRegions);

            // Act
            var service = new ApplicationService(pathDataService, fakeRegionService, contentRetriever, contentProcessor, taskHelper);
            var result = await service.GetApplicationAsync(Path).ConfigureAwait(false);

            // Assert
            Assert.Equal(defaultPathModel.Path, result.Path.Path);
            Assert.Equal(bodyAndFooterRegions.Count, result.Regions.Count());
            Assert.Equal(RequestBaseUrl, result.RootUrl);
        }

        [Fact]
        public async Task GetApplicationAsyncReturnsPathsAndRegionsAndNoRootUriWhenBodyRegionDoesNotExist()
        {
            // Arrange
            var fakeRegionModels = new List<RegionModel> { defaultBodyFooterRegion };
            var fakeRegionService = A.Fake<IRegionService>();
            A.CallTo(() => fakeRegionService.GetRegions(defaultPathModel.Path)).Returns(fakeRegionModels);

            // Act
            var service = new ApplicationService(pathDataService, fakeRegionService, contentRetriever, contentProcessor, taskHelper);
            var result = await service.GetApplicationAsync(Path).ConfigureAwait(false);

            // Assert
            Assert.Null(result.RootUrl);
            Assert.Equal(defaultPathModel.Path, result.Path.Path);
            Assert.Equal(fakeRegionModels.Count, result.Regions.Count());
        }

        [Fact]
        public async Task GetMarkupAsyncWhenRegionContentRetrievalTaskIsNotCompletedReturnOfflineHTML()
        {
            // Arrange
            var pageModel = new PageViewModel();
            mapper.Map(defaultApplicationModel, pageModel);

            var incompleteTask = A.Fake<ITaskHelper>();
            A.CallTo(() => incompleteTask.TaskCompletedSuccessfully(A<Task>.Ignored)).Returns(false);

            //Act
            var service = new ApplicationService(pathDataService, regionService, contentRetriever, contentProcessor, incompleteTask) { RequestBaseUrl = RequestBaseUrl };
            await service.GetMarkupAsync(defaultApplicationModel, "index", pageModel, string.Empty).ConfigureAwait(false);

            // Assert
            Assert.Equal(OfflineHTML, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(OfflineHTML, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);
        }

        [Theory]
        [MemberData(nameof(QueryStringParams))]
        public async Task GetMarkupAsyncMakesContentRequestWithQueryStringInCorrectPosition(string queryString, string expectedResult)
        {
            // Arrange
            var pageModel = new PageViewModel();
            mapper.Map(defaultApplicationModel, pageModel);

            //Act
            var service = new ApplicationService(pathDataService, regionService, contentRetriever, contentProcessor, taskHelper) { RequestBaseUrl = RequestBaseUrl };
            await service.GetMarkupAsync(defaultApplicationModel, Article, pageModel, queryString).ConfigureAwait(false);

            A.CallTo(() => contentRetriever.GetContent(expectedResult, defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl)).MustHaveHappenedOnceExactly();
        }
    }
}