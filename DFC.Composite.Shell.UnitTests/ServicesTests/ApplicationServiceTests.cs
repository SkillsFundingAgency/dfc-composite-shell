using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.Banner;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Utilities;

using FakeItEasy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationServiceTests
    {
        private const string RequestBaseUrl = "https://localhost";
        private const string ChildAppPath = "path1";
        private const string ChildAppData = "data1";
        private const string AppRegistryPathNameForPagesApp = "pages";
        private const string HeadRegionContent = "headRegionContent";
        private const string BodyRegionContent = "bodyRegionContent";
        private const string BodyFooterRegionContent = "bodyfooterRegionContent";
        private const string OfflineHtml = "<p>Offline HTML</p>";
        private const string Article = "article";

        private readonly ActionGetRequestModel childAppActionGetRequestModel = new ActionGetRequestModel { Path = ChildAppPath, Data = ChildAppData };
        private readonly IApplicationService applicationService;
        private readonly IMapper<ApplicationModel, PageViewModel> mapper;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly IBannerService bannerService;
        private readonly IContentRetriever contentRetriever;
        private readonly IContentProcessorService contentProcessor;
        private readonly MarkupMessages markupMessages;
        private readonly AppRegistrationModel defaultAppRegistrationModel;
        private readonly AppRegistrationModel? nullAppRegistrationModel = null;
        private readonly AppRegistrationModel pagesAppRegistrationModel;
        private readonly RegionModel defaultHeadRegion;
        private readonly RegionModel defaultBodyRegion;
        private readonly RegionModel defaultBodyFooterRegion;
        private readonly List<RegionModel> defaultRegions;
        private readonly ApplicationModel defaultApplicationModel;
        private readonly ApplicationModel pagesApplicationModel;
        private readonly ApplicationModel offlineApplicationModel;
        private readonly ApplicationModel offlineApplicationModelWithoutMarkup;
        private readonly PageViewModel defaultPageViewModel;
        private readonly List<KeyValuePair<string, string>> defaultFormPostParams;
        private readonly ITaskHelper taskHelper;

        public ApplicationServiceTests()
        {
            appRegistryDataService = A.Fake<IAppRegistryDataService>();
            bannerService = A.Fake<IBannerService>();
            mapper = new ApplicationToPageModelMapper(appRegistryDataService);
            contentRetriever = A.Fake<IContentRetriever>();
            contentProcessor = A.Fake<IContentProcessorService>();

            markupMessages = new MarkupMessages
            {
                AppOfflineHtml = "<h3>App offline</h3>",
                RegionOfflineHtml = new Dictionary<PageRegion, string>
                {
                    {
                        PageRegion.Head, "<h3>Head Region is offline</h3>"
                    },
                    {
                        PageRegion.Breadcrumb, "<h3>Breadcrumb Region is offline</h3>"
                    },
                    {
                        PageRegion.BodyTop, "<h3>BodyTop Region is offline</h3>"
                    },
                    {
                        PageRegion.Body, "<h3>Body Region is offline</h3>"
                    },
                    {
                        PageRegion.SidebarRight, "<h3>SidebarRight Region is offline</h3>"
                    },
                    {
                        PageRegion.SidebarLeft, "<h3>SidebarLeft Region is offline</h3>"
                    },
                    {
                        PageRegion.BodyFooter, "<h3>BodyFooter Region is offline</h3>"
                    },
                    {
                        PageRegion.HeroBanner, "<h3>HeroBanner Region is offline</h3>"
                    },
                },
            };

            var headRegionEndPoint = $"{RequestBaseUrl}/headRegionEndpoint";
            var bodyRegionEndPoint = $"{RequestBaseUrl}/bodyRegionEndpoint";
            var footerRegionEndPoint = $"{RequestBaseUrl}/footerRegionEndpoint";

            defaultHeadRegion = new RegionModel { PageRegion = PageRegion.Head, RegionEndpoint = headRegionEndPoint, IsHealthy = true, OfflineHtml = OfflineHtml };
            defaultBodyRegion = new RegionModel { PageRegion = PageRegion.Body, RegionEndpoint = bodyRegionEndPoint, IsHealthy = true, OfflineHtml = OfflineHtml };
            defaultBodyFooterRegion = new RegionModel { PageRegion = PageRegion.BodyFooter, RegionEndpoint = footerRegionEndPoint, IsHealthy = true, OfflineHtml = OfflineHtml };
            defaultRegions = new List<RegionModel>
            {
                defaultHeadRegion,
                defaultBodyRegion,
                defaultBodyFooterRegion,
            };
            defaultAppRegistrationModel = new AppRegistrationModel { Path = ChildAppPath, TopNavigationOrder = 1, IsOnline = true, Regions = defaultRegions };
            pagesAppRegistrationModel = new AppRegistrationModel { Path = AppRegistryPathNameForPagesApp, TopNavigationOrder = 1, IsOnline = true, Regions = defaultRegions };

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

            defaultApplicationModel = new ApplicationModel { AppRegistrationModel = defaultAppRegistrationModel, Article = "index" };
            pagesApplicationModel = new ApplicationModel { AppRegistrationModel = pagesAppRegistrationModel };
            offlineApplicationModel = new ApplicationModel { AppRegistrationModel = new AppRegistrationModel { IsOnline = false, OfflineHtml = OfflineHtml } };
            offlineApplicationModelWithoutMarkup = new ApplicationModel { AppRegistrationModel = new AppRegistrationModel { IsOnline = false, OfflineHtml = null } };

            A.CallTo(() => appRegistryDataService.GetAppRegistrationModel($"{ChildAppPath}/{ChildAppData}")).Returns(nullAppRegistrationModel);
            A.CallTo(() => appRegistryDataService.GetAppRegistrationModel(ChildAppPath)).Returns(defaultAppRegistrationModel);
            A.CallTo(() => appRegistryDataService.GetAppRegistrationModel(AppRegistryPathNameForPagesApp)).Returns(pagesAppRegistrationModel);
            A.CallTo(() => contentRetriever.GetContent($"{defaultHeadRegion.RegionEndpoint}/index", defaultApplicationModel.AppRegistrationModel.Path, defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(HeadRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyRegion.RegionEndpoint}/index", defaultApplicationModel.AppRegistrationModel.Path, defaultBodyRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(BodyRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}", defaultApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(BodyFooterRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/index", defaultApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(BodyFooterRegionContent);

            A.CallTo(() => contentProcessor.Process(HeadRegionContent, A<string>.Ignored, A<string>.Ignored)).Returns(HeadRegionContent);
            A.CallTo(() => contentProcessor.Process(BodyRegionContent, A<string>.Ignored, A<string>.Ignored)).Returns(BodyRegionContent);
            A.CallTo(() => contentProcessor.Process(BodyFooterRegionContent, A<string>.Ignored, A<string>.Ignored)).Returns(BodyFooterRegionContent);

            defaultFormPostParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("formParam1", "testvalue") };

            taskHelper = A.Fake<ITaskHelper>();
            A.CallTo(() => taskHelper.TaskCompletedSuccessfully(A<Task>.Ignored)).Returns(true);

            applicationService = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages) { RequestBaseUrl = RequestBaseUrl };
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
            await mapper.Map(defaultApplicationModel, pageModel);

            //Act
            await applicationService.GetMarkupAsync(defaultApplicationModel, pageModel, string.Empty, string.Empty, new HeaderDictionary());

            //Assert
            Assert.Equal(defaultRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(HeadRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Head).Content.Value);
            Assert.Equal(BodyRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.GetContent($"{defaultHeadRegion.RegionEndpoint}/index", defaultApplicationModel.AppRegistrationModel.Path, defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyRegion.RegionEndpoint}/index", defaultApplicationModel.AppRegistrationModel.Path, defaultBodyRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/index", defaultApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMarkupAsyncForOnlineApplicationWhenBodyRegionIsEmpty()
        {
            // Arrange
            var fakeBodyRegionEndPoint = string.Empty;
            var fakeBodyRegion = new RegionModel { PageRegion = PageRegion.Body, RegionEndpoint = fakeBodyRegionEndPoint, IsHealthy = true };
            var fakeRegions = new List<RegionModel> { defaultHeadRegion, fakeBodyRegion, defaultBodyFooterRegion };
            var fakeApplicationModel = new ApplicationModel { AppRegistrationModel = defaultAppRegistrationModel, Article = "index" };
            fakeApplicationModel.AppRegistrationModel.Regions = fakeRegions;
            var pageModel = new PageViewModel();
            await mapper.Map(fakeApplicationModel, pageModel);

            A.CallTo(() => contentRetriever.GetContent($"{fakeBodyRegion.RegionEndpoint}/index", fakeApplicationModel.AppRegistrationModel.Path, fakeBodyRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(BodyRegionContent);

            //Act
            await applicationService.GetMarkupAsync(fakeApplicationModel, pageModel, string.Empty, string.Empty, new HeaderDictionary());

            //Assert
            Assert.Equal(fakeRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(HeadRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Head).Content.Value);
            Assert.Equal(string.Empty, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.GetContent($"{defaultHeadRegion.RegionEndpoint}/index", fakeApplicationModel.AppRegistrationModel.Path, defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{fakeBodyRegion.RegionEndpoint}/index", fakeApplicationModel.AppRegistrationModel.Path, fakeBodyRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/index", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetMarkupAsyncWhenApplicationIsOfflineThenOfflineHtmlIsReturned()
        {
            await applicationService.GetMarkupAsync(offlineApplicationModel, defaultPageViewModel, string.Empty, string.Empty, new HeaderDictionary());

            Assert.Equal(OfflineHtml, defaultPageViewModel.PageRegionContentModels.First().Content.ToString());
        }

        [Fact]
        public async Task GetMarkupAsyncWhenApplicationIsOfflineThenMarkupMessagesOfflineHtmlIsReturned()
        {
            await applicationService.GetMarkupAsync(offlineApplicationModelWithoutMarkup, defaultPageViewModel, string.Empty, string.Empty, new HeaderDictionary());

            Assert.Equal(markupMessages.AppOfflineHtml, defaultPageViewModel.PageRegionContentModels.First().Content.ToString());
        }

        [Fact]
        public async Task GetMarkupAsyncWhenApplicationModelIsNullThenArgumentNullExceptionThrown()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await applicationService.GetMarkupAsync(null, defaultPageViewModel, string.Empty, string.Empty, new HeaderDictionary()));
        }

        [Fact]
        public async Task GetMarkupAsyncWhenPageViewModelIsNullThenArgumentNullExceptionThrown()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await applicationService.GetMarkupAsync(defaultApplicationModel, null, string.Empty, string.Empty, new HeaderDictionary()));
        }

        [Fact]
        public async Task PostMarkupAsyncWhenApplicationPathIsOfflineThenOfflineHtmlIsReturned()
        {
            await applicationService.PostMarkupAsync(offlineApplicationModel, defaultFormPostParams, defaultPageViewModel, string.Empty, new HeaderDictionary());

            Assert.Equal(OfflineHtml, defaultPageViewModel.PageRegionContentModels.First().Content.ToString());
        }

        [Fact]
        public async Task PostMarkupAsyncWhenApplicationPathIsOfflineThenMarkupMessagesOfflineHtmlIsReturned()
        {
            await applicationService.PostMarkupAsync(offlineApplicationModelWithoutMarkup, defaultFormPostParams, defaultPageViewModel, string.Empty, new HeaderDictionary());

            Assert.Equal(markupMessages.AppOfflineHtml, defaultPageViewModel.PageRegionContentModels.First().Content.ToString());
        }

        [Fact]
        public async Task PostMarkupAsyncForOnlineApplication()
        {
            // Arrange
            var footerAndBodyRegions = new List<RegionModel> { defaultHeadRegion, defaultBodyRegion, defaultBodyFooterRegion };
            var fakeApplicationModel = new ApplicationModel { AppRegistrationModel = defaultAppRegistrationModel, Article = Article };
            fakeApplicationModel.AppRegistrationModel.Regions = footerAndBodyRegions;

            var pageModel = new PageViewModel();
            await mapper.Map(fakeApplicationModel, pageModel);

            A.CallTo(() => contentRetriever.PostContent($"{defaultBodyRegion.RegionEndpoint}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyRegion, defaultFormPostParams, RequestBaseUrl)).Returns(BodyRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(BodyFooterRegionContent);

            // Act
            await applicationService.PostMarkupAsync(fakeApplicationModel, defaultFormPostParams, pageModel, string.Empty, new HeaderDictionary());

            //Assert
            Assert.Equal(footerAndBodyRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(BodyRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.PostContent($"{defaultBodyRegion.RegionEndpoint}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyRegion, defaultFormPostParams, RequestBaseUrl)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PostMarkupAsyncForOnlineApplicationWhenBodyRegionEndpointIsEmptyThenContentNotPosted()
        {
            // Arrange
            var fakeBodyRegionEndpoint = string.Empty;
            var fakeBodyRegion = new RegionModel { PageRegion = PageRegion.Body, RegionEndpoint = fakeBodyRegionEndpoint, IsHealthy = true };
            var fakeRegions = new List<RegionModel> { defaultHeadRegion, fakeBodyRegion, defaultBodyFooterRegion };
            var fakeApplicationModel = new ApplicationModel { AppRegistrationModel = defaultAppRegistrationModel, Article = Article };
            fakeApplicationModel.AppRegistrationModel.Regions = fakeRegions;
            var pageModel = new PageViewModel();
            await mapper.Map(fakeApplicationModel, pageModel);

            A.CallTo(() => contentRetriever.PostContent($"{RequestBaseUrl}/{fakeApplicationModel.AppRegistrationModel.Path}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, fakeBodyRegion, defaultFormPostParams, RequestBaseUrl)).Returns(BodyRegionContent);
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).Returns(BodyFooterRegionContent);

            // Act
            await applicationService.PostMarkupAsync(fakeApplicationModel, defaultFormPostParams, pageModel, string.Empty, new HeaderDictionary());

            //Assert
            Assert.Equal(fakeRegions.Count, pageModel.PageRegionContentModels.Count);
            Assert.Equal(string.Empty, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(BodyFooterRegionContent, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);

            A.CallTo(() => contentRetriever.PostContent($"{RequestBaseUrl}/{fakeApplicationModel.AppRegistrationModel.Path}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, fakeBodyRegion, defaultFormPostParams, RequestBaseUrl)).MustNotHaveHappened();
            A.CallTo(() => contentRetriever.GetContent($"{defaultBodyFooterRegion.RegionEndpoint}/{Article}", fakeApplicationModel.AppRegistrationModel.Path, defaultBodyFooterRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetApplicationAsyncReturnsEmptyApplicationModelWhenPathNotFound()
        {
            // Arrange
            var localAppRegistryDataService = A.Fake<IAppRegistryDataService>();
            A.CallTo(() => localAppRegistryDataService.GetAppRegistrationModel(A<string>.Ignored)).Returns((AppRegistrationModel)null);

            // Act
            var service = new ApplicationService(localAppRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages);
            var result = await service.GetApplicationAsync(childAppActionGetRequestModel);

            // Assert
            Assert.Null(result.RootUrl);
            Assert.Null(result.AppRegistrationModel);
        }

        [Fact]
        public async Task GetApplicationAsyncReturnsPathsAndRegionsAndRootUriPagesApp()
        {
            // Arrange
            var bodyAndFooterRegions = new List<RegionModel>
            {
                defaultBodyRegion,
                defaultBodyFooterRegion,
            };
            var thisChildAppActionGetRequestModel = new ActionGetRequestModel { Path = "help-me", Data = string.Empty };
            var appRegistryModel = appRegistryDataService.GetAppRegistrationModel(AppRegistryPathNameForPagesApp).Result;
            appRegistryModel.Regions = bodyAndFooterRegions;
            appRegistryModel.PageLocations = new Dictionary<Guid, PageLocationModel> { { Guid.NewGuid(), new PageLocationModel { Locations = new List<string> { "/help-me" } } } };

            // Act
            var service = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages);
            var result = await service.GetApplicationAsync(thisChildAppActionGetRequestModel);

            // Assert
            Assert.Equal(AppRegistryPathNameForPagesApp, result.AppRegistrationModel.Path);
            Assert.Equal(bodyAndFooterRegions.Count, result.AppRegistrationModel.Regions.Count);
            Assert.Equal(RequestBaseUrl, result.RootUrl);
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
            appRegistryDataService.GetAppRegistrationModel(ChildAppPath).Result.Regions = bodyAndFooterRegions;

            // Act
            var service = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages);
            var result = await service.GetApplicationAsync(childAppActionGetRequestModel);

            // Assert
            Assert.Equal(defaultAppRegistrationModel.Path, result.AppRegistrationModel.Path);
            Assert.Equal(bodyAndFooterRegions.Count, result.AppRegistrationModel.Regions.Count);
            Assert.Equal(RequestBaseUrl, result.RootUrl);
        }

        [Fact]
        public async Task GetApplicationAsyncReturnsPathsAndRegionsAndNoRootUriWhenBodyRegionDoesNotExist()
        {
            // Arrange
            var fakeRegionModels = new List<RegionModel> { defaultBodyFooterRegion };
            appRegistryDataService.GetAppRegistrationModel(ChildAppPath).Result.Regions = fakeRegionModels;

            // Act
            var service = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages);
            var result = await service.GetApplicationAsync(childAppActionGetRequestModel);

            // Assert
            Assert.Null(result.RootUrl);
            Assert.Equal(defaultAppRegistrationModel.Path, result.AppRegistrationModel.Path);
            Assert.Equal(fakeRegionModels.Count, result.AppRegistrationModel.Regions.Count);
        }

        [Fact]
        public async Task GetMarkupAsyncWhenRegionContentRetrievalTaskIsNotCompletedReturnOfflineHtml()
        {
            // Arrange
            var pageModel = new PageViewModel();
            await mapper.Map(defaultApplicationModel, pageModel);

            var incompleteTask = A.Fake<ITaskHelper>();
            A.CallTo(() => incompleteTask.TaskCompletedSuccessfully(A<Task>.Ignored)).Returns(false);

            //Act
            var service = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, incompleteTask, bannerService, markupMessages) { RequestBaseUrl = RequestBaseUrl };
            await service.GetMarkupAsync(defaultApplicationModel, pageModel, string.Empty, string.Empty, new HeaderDictionary());

            // Assert
            Assert.Equal(OfflineHtml, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body).Content.Value);
            Assert.Equal(OfflineHtml, pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.BodyFooter).Content.Value);
        }

        [Theory]
        [MemberData(nameof(QueryStringParams))]
        public async Task GetMarkupAsyncMakesContentRequestWithQueryStringInCorrectPosition(string queryString, string expectedResult)
        {
            // Arrange
            var pageModel = new PageViewModel();
            defaultApplicationModel.Article = Article;
            await mapper.Map(defaultApplicationModel, pageModel);

            //Act
            var service = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages) { RequestBaseUrl = RequestBaseUrl };
            await service.GetMarkupAsync(defaultApplicationModel, pageModel, string.Empty, queryString, new HeaderDictionary());

            A.CallTo(() => contentRetriever.GetContent(expectedResult, defaultApplicationModel.AppRegistrationModel.Path, defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [MemberData(nameof(QueryStringParams))]
        public async Task GetMarkupAsyncMakesRequestToBannersAppWithRequest(string queryString, string expectedResult)
        {
            // Arrange
            var pageModel = new PageViewModel();
            defaultApplicationModel.Article = Article;
            await mapper.Map(defaultApplicationModel, pageModel);
            var requestPath = "/job-profiles/admin";

            // Act
            var service = new ApplicationService(appRegistryDataService, contentRetriever, contentProcessor, taskHelper, bannerService, markupMessages) { RequestBaseUrl = RequestBaseUrl };
            await service.GetMarkupAsync(defaultApplicationModel, pageModel, requestPath, queryString, new HeaderDictionary());

            // Assert
            A.CallTo(() => bannerService.GetPageBannersAsync(requestPath)).MustHaveHappenedOnceExactly();
            A.CallTo(() => contentRetriever.GetContent(expectedResult, defaultApplicationModel.AppRegistrationModel.Path, defaultHeadRegion, A<bool>.Ignored, RequestBaseUrl, A<IHeaderDictionary>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}