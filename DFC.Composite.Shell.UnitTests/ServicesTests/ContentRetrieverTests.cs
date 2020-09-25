using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
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
        private readonly HttpClient httpClient;
        private readonly FakeHttpMessageHandler fakeHttpMessageHandler;
        private readonly ILogger<ContentRetriever> logger;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly IHttpResponseMessageHandler httpResponseMessageHandler;
        private readonly MarkupMessages markupMessages;
        private readonly List<KeyValuePair<string, string>> defaultFormPostParams;

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
            httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            defaultFormPostParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("formParam1", "test value") };

            logger = A.Fake<ILogger<ContentRetriever>>();
            appRegistryDataService = A.Fake<IAppRegistryDataService>();
            httpResponseMessageHandler = A.Fake<IHttpResponseMessageHandler>();

            markupMessages = new MarkupMessages { AppOfflineHtml = "<h3>App offline</h3>", RegionOfflineHtml = "<h3>Region offline</h3>" };

            defaultService = new ContentRetriever(httpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);
        }

        ~ContentRetrieverTests()
        {
            fakeHttpMessageHandler.Dispose();
            httpResponse.Dispose();
            httpClient.Dispose();
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

            var result = await defaultService.GetContent("someUrl", "path", model, true, "baseUrl").ConfigureAwait(false);

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

            var result = await defaultService.GetContent("someUrl", "path", model, true, "baseUrl").ConfigureAwait(false);

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task GetContentWhenRegionIsNotHealthyReturnMarkupMessageOfflineHtml()
        {
            var model = new RegionModel
            {
                IsHealthy = false,
                OfflineHtml = null,
            };

            var result = await defaultService.GetContent("someUrl", "path", model, true, "baseUrl").ConfigureAwait(false);

            Assert.Equal(markupMessages.RegionOfflineHtml, result);
        }

        [Fact]
        public async Task GetContentWhenRegionReturnsRedirectResponseThenFollowRedirect()
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

            var service = new ContentRetriever(redirectHttpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);

            await service.GetContent("someUrl", "path", model, true, "baseUrl").ConfigureAwait(false);

            A.CallTo(() => httpResponseMessageHandler.Process(null)).MustHaveHappened();

            fakeRedirectHttpMessageHandler.Dispose();
            redirectHttpResponse.Dispose();
            redirectHttpClient.Dispose();
        }

        [Fact]
        public async Task GetContentWhenRegionIsNullCreateException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await defaultService.GetContent("http://someUrl", null, null, false, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);
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

            var service = new ContentRetriever(redirectHttpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);

            await Assert.ThrowsAnyAsync<RedirectException>(async () => await service.GetContent("http://someUrl", "path", model, false, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);

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

            var service = new ContentRetriever(httpClient, logger, fakeRegionService, fakeRedirectHttpMessageHandler, markupMessages);

            await service.GetContent("someUrl", "path", model, true, "baseUrl").ConfigureAwait(false);

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

            var service = new ContentRetriever(httpClient, logger, fakeRegionService, fakeRedirectHttpMessageHandler, markupMessages);

            var result = await service.GetContent("someUrl", "path", model, true, "baseUrl").ConfigureAwait(false);

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustNotHaveHappened();

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task PostContentWhenRegionIsNullCreateException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await defaultService.PostContent("http://someUrl", null, null, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);
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

            var service = new ContentRetriever(postHttpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);

            await Assert.ThrowsAnyAsync<RedirectException>(async () => await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);

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

            var service = new ContentRetriever(postHttpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);

            var result = await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

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

            var result = await defaultService.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

            Assert.Equal(OfflineHtml, result);
        }

        [Fact]
        public async Task PostContentWhenRegionIsNotHealthyReturnMarkupMessagesOfflineHtml()
        {
            var model = new RegionModel
            {
                IsHealthy = false,
                OfflineHtml = null,
            };

            var result = await defaultService.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

            Assert.Equal(markupMessages.RegionOfflineHtml, result);
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

            var service = new ContentRetriever(httpClient, fakeLogger, fakeRegionService, httpResponseMessageHandler, markupMessages);

            await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

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

            var service = new ContentRetriever(httpClient, fakeLogger, fakeRegionService, httpResponseMessageHandler, markupMessages);

            var result = await service.PostContent("http://someUrl", "path", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

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
            var httpClient = httpHandler.ToHttpClient();
            httpHandler.When(HttpMethod.Post, postUrl).Respond(x => httpResponseMessage);
            var contentRetriever = new ContentRetriever(httpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);

            await Assert.ThrowsAsync<RedirectException>(async () => await contentRetriever.PostContent(postUrl, "path", model, defaultFormPostParams, baseUrl).ConfigureAwait(false)).ConfigureAwait(false);

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
            var httpClient = httpHandler.ToHttpClient();
            httpHandler.When(HttpMethod.Post, postUrl).Respond(x => httpResponseMessage);
            var contentRetriever = new ContentRetriever(httpClient, logger, appRegistryDataService, httpResponseMessageHandler, markupMessages);

            var ex = await Assert.ThrowsAsync<RedirectException>(async () => await contentRetriever.PostContent(postUrl, "path", model, defaultFormPostParams, baseUrl).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.Equal("https://base/baseurl/redirecturl", ex.Location.AbsoluteUri);
            Assert.Equal(postUrl, ex.OldLocation.AbsoluteUri);

            httpHandler.Dispose();
            httpResponseMessage.Dispose();
        }
    }
}