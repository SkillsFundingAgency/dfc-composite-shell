using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ContentRetrieverTests
    {
        private const string DummyChildAppContent = "<p>Some Content From Child App</p>";
        private readonly ContentRetriever defaultService;
        private readonly HttpResponseMessage httpResponse;
        private readonly IUriSpecifcHttpClientFactory httpClientFactory;
        private readonly HttpClient httpClient;
        private readonly FakeHttpMessageHandler fakeHttpMessageHandler;
        private readonly ILogger<ContentRetriever> logger;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly IHttpResponseMessageHandler httpResponseMessageHandler;
        private readonly MarkupMessages markupMessages;
        private readonly List<KeyValuePair<string, string>> defaultFormPostParams;
        private readonly IMemoryCache memoryCache;
        private readonly string baseUri = "http://baseUrl";

        public ContentRetrieverTests()
        {
            httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(DummyChildAppContent),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);

            httpClientFactory = A.Fake<IUriSpecifcHttpClientFactory>();
            httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            A.CallTo(() => httpClientFactory.GetClientForRegionEndpoint(A<string>.Ignored)).Returns(httpClient);

            defaultFormPostParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("formParam1", "test value") };

            logger = A.Fake<ILogger<ContentRetriever>>();
            appRegistryDataService = A.Fake<IAppRegistryDataService>();
            httpResponseMessageHandler = A.Fake<IHttpResponseMessageHandler>();

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

            memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            defaultService = new ContentRetriever(httpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);
        }

        ~ContentRetrieverTests()
        {
            fakeHttpMessageHandler.Dispose();
            httpResponse.Dispose();
            httpClient.Dispose();
            memoryCache.Dispose();
        }

        [Fact]
        public async Task GetContentWhenContentRetrievedFromChildAppThenContentStringReturned()
        {
            var model = new RegionModel
            {
                IsHealthy = true,
                RegionEndpoint = "SomeRegionEndpoint",
                PageRegion = PageRegion.Body,
            };

            var result = await defaultService.GetContent("someUrl", "path", model, true, "baseUrl");

            Assert.Equal(DummyChildAppContent, result);
        }

        [Fact]
        public async Task GetContentWhenRegionIsNotHealthyReturnOfflineHtml()
        {
            const string OfflineHtml = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = false,
                OfflineHtml = OfflineHtml,
            };

            var result = await defaultService.GetContent("someUrl", "path", model, true, "baseUrl");

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task GetContentWhenRegionIsNotHealthyReturnMarkupMessageOfflineHtml()
        {
            var model = new RegionModel
            {
                PageRegion = PageRegion.BodyTop,
                IsHealthy = false,
                OfflineHtml = null,
            };

            var result = await defaultService.GetContent("someUrl", "path", model, true, "baseUrl");

            Assert.Equal(markupMessages.RegionOfflineHtml[model.PageRegion], result);
        }

        [Fact]
        public async Task GetContentWhenRegionReturnsRedirectResponseThenFollowRedirect()
        {
            using var redirectHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Redirect,
                Content = new StringContent(DummyChildAppContent),
            };
            redirectHttpResponse.Headers.Location = new Uri("http://someUrl");

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(redirectHttpResponse);

            using var fakeRedirectHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var redirectHttpClient = new HttpClient(fakeRedirectHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            var model = new RegionModel
            {
                IsHealthy = true,
            };

            using var httpHandler = new MockHttpMessageHandler();
            httpHandler.When(HttpMethod.Get, "http://someUrl").Respond(_ => redirectHttpResponse);

            var localHttpClientFactory = A.Fake<IUriSpecifcHttpClientFactory>();
            A.CallTo(() => localHttpClientFactory.GetClientForRegionEndpoint(A<string>.Ignored)).Returns(httpHandler.ToHttpClient());

            var service = new ContentRetriever(localHttpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);

            await service.GetContent("http://someUrl", "path", model, true, baseUri);

            A.CallTo(() => httpResponseMessageHandler.Process(null)).MustHaveHappened();
        }

        [Fact]
        public async Task GetContentWhenRegionIsNullCreateException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await defaultService.GetContent("http://someUrl", null, null, false, "http://baseUrl"));
        }

        [Fact]
        public async Task GetContentWhenRegionReturnsRedirectResponseThenThrowRedirectException()
        {
            var redirectHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Redirect,
                Content = new StringContent(DummyChildAppContent),
            };
            redirectHttpResponse.Headers.Location = new Uri("http://SomeLocation");

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(redirectHttpResponse);

            var fakeRedirectHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var redirectHttpClient = new HttpClient(fakeRedirectHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            var model = new RegionModel
            {
                IsHealthy = true,
            };

            using var httpHandler = new MockHttpMessageHandler();
            httpHandler.When(HttpMethod.Get, "http://someUrl").Respond(_ => redirectHttpResponse);

            var localHttpClientFactory = A.Fake<IUriSpecifcHttpClientFactory>();
            A.CallTo(() => localHttpClientFactory.GetClientForRegionEndpoint(A<string>.Ignored)).Returns(httpHandler.ToHttpClient());

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(localHttpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);

            await Assert.ThrowsAnyAsync<RedirectException>(async () => await service.GetContent("http://someUrl", "path", model, false, "http://baseUrl"));

            fakeRedirectHttpMessageHandler.Dispose();
            redirectHttpResponse.Dispose();
            redirectHttpClient.Dispose();
        }

        [Fact]
        public async Task GetContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsRequiredThenRegionStateUpdated()
        {
            var model = new RegionModel
            {
                IsHealthy = true,
                HealthCheckRequired = true,
            };

            var fakeRedirectHttpMessageHandler = A.Fake<IHttpResponseMessageHandler>();
            A.CallTo(() => fakeRedirectHttpMessageHandler.Process(A<HttpResponseMessage>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IAppRegistryDataService>();

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(httpClientFactory, logger, fakeRegionService, fakeRedirectHttpMessageHandler, markupMessages, memoryCache);

            await service.GetContent("someUrl", "path", model, true, "baseUrl");

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsNotRequiredThenReturnOfflineHtml()
        {
            const string OfflineHtml = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = true,
                OfflineHtml = OfflineHtml,
            };

            var fakeRedirectHttpMessageHandler = A.Fake<IHttpResponseMessageHandler>();
            A.CallTo(() => fakeRedirectHttpMessageHandler.Process(A<HttpResponseMessage>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IAppRegistryDataService>();

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(httpClientFactory, logger, fakeRegionService, fakeRedirectHttpMessageHandler, markupMessages, memoryCache);

            var result = await service.GetContent("someUrl", "path", model, true, "baseUrl");

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustNotHaveHappened();

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task PostContentWhenRegionIsNullCreateException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await defaultService.PostContent("http://someUrl", null, null, defaultFormPostParams, "http://baseUrl"));
        }

        [Fact]
        public async Task PostContentWhenResponseIsStatusFoundThrowRedirectException()
        {
            var model = new RegionModel
            {
                IsHealthy = true,
                RegionEndpoint = "SomeRegionEndpoint",
                PageRegion = PageRegion.Body,
            };

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Found,
                Content = new StringContent(DummyChildAppContent),
            };
            httpResponseMessage.Headers.Location = new Uri("http://SomeLocation");

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponseMessage);

            var fakeRedirectHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var postHttpClient = new HttpClient(fakeRedirectHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            using var httpHandler = new MockHttpMessageHandler();
            httpHandler.When(HttpMethod.Post, "http://someUrl").Respond(_ => httpResponseMessage);

            var localHttpClientFactory = A.Fake<IUriSpecifcHttpClientFactory>();
            A.CallTo(() => localHttpClientFactory.GetClientForRegionEndpoint(A<string>.Ignored)).Returns(httpHandler.ToHttpClient());

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(localHttpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);

            await Assert.ThrowsAnyAsync<RedirectException>(async () => await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl"));

            httpResponseMessage.Dispose();
            fakeRedirectHttpMessageHandler.Dispose();
            postHttpClient.Dispose();
        }

        [Fact]
        public async Task PostContentWhenResponseIsStatusOKReturnsContent()
        {
            var model = new RegionModel
            {
                IsHealthy = true,
                RegionEndpoint = "SomeRegionEndpoint",
                PageRegion = PageRegion.Body,
            };

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(DummyChildAppContent),
            };
            httpResponseMessage.Headers.Location = new Uri("http://SomeLocation");

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponseMessage);

            var fakeRedirectHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var postHttpClient = new HttpClient(fakeRedirectHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(httpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);

            var result = await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl");

            Assert.Equal(DummyChildAppContent, result);

            httpResponseMessage.Dispose();
            fakeRedirectHttpMessageHandler.Dispose();
            postHttpClient.Dispose();
        }

        [Fact]
        public async Task PostContentWhenRegionIsNotHealthyReturnOfflineHtml()
        {
            const string OfflineHtml = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = false,
                OfflineHtml = OfflineHtml,
            };

            var result = await defaultService.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl");

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task PostContentWhenRegionIsNotHealthyReturnMarkupMessagesOfflineHtml()
        {
            var model = new RegionModel
            {
                PageRegion = PageRegion.BodyTop,
                IsHealthy = false,
                OfflineHtml = null,
            };

            var result = await defaultService.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl");

            Assert.Equal(markupMessages.RegionOfflineHtml[model.PageRegion], result);
        }

        [Fact(Skip = "Needs revisiting as part of DFC-11808")]
        public async Task PostContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsRequiredThenRegionStateUpdated()
        {
            var model = new RegionModel
            {
                IsHealthy = true,
                HealthCheckRequired = true,
            };

            var fakeLogger = A.Fake<ILogger<ContentRetriever>>();

            A.CallTo(() => fakeLogger.Log(LogLevel.Information, 0, A<IReadOnlyList<KeyValuePair<string, object>>>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IAppRegistryDataService>();

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(httpClientFactory, fakeLogger, fakeRegionService, httpResponseMessageHandler, markupMessages, memoryCache);

            await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl");

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustHaveHappenedOnceExactly();
        }

        [Fact(Skip = "Needs revisiting as part of DFC-11808")]
        public async Task PostContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsNotRequiredThenReturnOfflineHtml()
        {
            const string OfflineHtml = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = true,
                OfflineHtml = OfflineHtml,
            };

            var fakeLogger = A.Fake<ILogger<ContentRetriever>>();

            A.CallTo(() => fakeLogger.Log(LogLevel.Information, 0, A<IReadOnlyList<KeyValuePair<string, object>>>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IAppRegistryDataService>();

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var service = new ContentRetriever(httpClientFactory, fakeLogger, fakeRegionService, httpResponseMessageHandler, markupMessages, memoryCache);

            var result = await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl");

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustNotHaveHappened();

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task WhenPostContentIssuesARedirectThenHandlerProcessesRequest()
        {
            var baseUrl = "https://base/baseurl";
            var postUrl = "https://base/posturl";
            var redirectUrl = "https://child1/redirecturl";
            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Redirect,
            };
            httpResponseMessage.Headers.Location = new Uri(redirectUrl);

            var model = new RegionModel
            {
                IsHealthy = true,
            };
            var httpHandler = new MockHttpMessageHandler();
            httpHandler.When(HttpMethod.Post, postUrl).Respond(x => httpResponseMessage);

            var localHttpClientFactory = A.Fake<IUriSpecifcHttpClientFactory>();
            A.CallTo(() => localHttpClientFactory.GetClientForRegionEndpoint(A<string>.Ignored)).Returns(httpHandler.ToHttpClient());

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var contentRetriever = new ContentRetriever(localHttpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);

            await Assert.ThrowsAsync<RedirectException>(async () => await contentRetriever.PostContent(postUrl, "path", model, defaultFormPostParams, baseUrl));

            A.CallTo(() => httpResponseMessageHandler.Process(A<HttpResponseMessage>.That.Matches(x => x.StatusCode == HttpStatusCode.Redirect))).MustHaveHappenedOnceExactly();

            httpHandler.Dispose();
            httpResponseMessage.Dispose();
        }

        [Fact]
        public async Task WhenPostContentIssuesARedirectThenRedirectExceptionIsThrown()
        {
            var baseUrl = "https://base/baseurl";
            var postUrl = "https://base/posturl";
            var redirectUrl = "https://child1/redirecturl";
            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Redirect,
            };
            httpResponseMessage.Headers.Location = new Uri(redirectUrl);

            var model = new RegionModel
            {
                IsHealthy = true,
            };
            var httpHandler = new MockHttpMessageHandler();
            httpHandler.When(HttpMethod.Post, postUrl).Respond(x => httpResponseMessage);

            var localHttpClientFactory = A.Fake<IUriSpecifcHttpClientFactory>();
            A.CallTo(() => localHttpClientFactory.GetClientForRegionEndpoint(A<string>.Ignored)).Returns(httpHandler.ToHttpClient());

            using var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var contentRetriever = new ContentRetriever(localHttpClientFactory, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages, memoryCache);

            var ex = await Assert.ThrowsAsync<RedirectException>(async () => await contentRetriever.PostContent(postUrl, "path", model, defaultFormPostParams, baseUrl));
            Assert.Equal("https://base/baseurl/redirecturl", ex.Location.AbsoluteUri);
            Assert.Equal(postUrl, ex.OldLocation.AbsoluteUri);

            httpHandler.Dispose();
            httpResponseMessage.Dispose();
        }
    }
}