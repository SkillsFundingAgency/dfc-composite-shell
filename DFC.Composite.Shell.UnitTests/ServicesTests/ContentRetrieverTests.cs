using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Polly.CircuitBreaker;
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
        private readonly IRegionService regionService;
        private readonly IHttpResponseMessageHandler httpResponseMessageHandler;
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

            defaultFormPostParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("formParam1", "testvalue") };

            logger = A.Fake<ILogger<ContentRetriever>>();
            regionService = A.Fake<IRegionService>();
            httpResponseMessageHandler = A.Fake<IHttpResponseMessageHandler>();

            defaultService = new ContentRetriever(httpClient, logger, regionService, httpResponseMessageHandler);
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

            var result = await defaultService.GetContent("someUrl", model, true, "baseUrl").ConfigureAwait(false);

            Assert.Equal(DummyChildAppContent, result);
        }

        [Fact]
        public async Task GetContentWhenRegionIsNotHealthyReturnOfflineHTML()
        {
            const string offlineHTML = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = false,
                OfflineHTML = offlineHTML,
            };

            var result = await defaultService.GetContent("someUrl", model, true, "baseUrl").ConfigureAwait(false);

            Assert.Equal(offlineHTML, result);
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

            var service = new ContentRetriever(redirectHttpClient, logger, regionService, httpResponseMessageHandler);

            await service.GetContent("someUrl", model, true, "baseUrl").ConfigureAwait(false);

            A.CallTo(() => logger.Log(LogLevel.Warning, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappened();

            fakeRedirectHttpMessageHandler.Dispose();
            redirectHttpResponse.Dispose();
            redirectHttpClient.Dispose();
        }

        [Fact]
        public async Task GetContentWhenRegionIsNullCreateException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await defaultService.GetContent("http://someUrl", null, false, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);
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

            var service = new ContentRetriever(redirectHttpClient, logger, regionService, httpResponseMessageHandler);

            await Assert.ThrowsAnyAsync<RedirectException>(async () => await service.GetContent("http://someUrl", model, false, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);

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
                HeathCheckRequired = true,
            };

            var fakeRedirectHttpMessageHandler = A.Fake<IHttpResponseMessageHandler>();
            A.CallTo(() => fakeRedirectHttpMessageHandler.Process(A<HttpResponseMessage>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IRegionService>();

            var service = new ContentRetriever(httpClient, logger, fakeRegionService, fakeRedirectHttpMessageHandler);

            await service.GetContent("someUrl", model, true, "baseUrl").ConfigureAwait(false);

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsNotRequiredThenReturnOfflineHTML()
        {
            const string offlineHTML = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = true,
                OfflineHTML = offlineHTML,
            };

            var fakeRedirectHttpMessageHandler = A.Fake<IHttpResponseMessageHandler>();
            A.CallTo(() => fakeRedirectHttpMessageHandler.Process(A<HttpResponseMessage>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IRegionService>();

            var service = new ContentRetriever(httpClient, logger, fakeRegionService, fakeRedirectHttpMessageHandler);

            var result = await service.GetContent("someUrl", model, true, "baseUrl").ConfigureAwait(false);

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustNotHaveHappened();

            Assert.Equal(offlineHTML, result);
        }

        [Fact]
        public async Task PostContentWhenRegionIsNullCreateException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await defaultService.PostContent("http://someUrl", null, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);
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

            var service = new ContentRetriever(postHttpClient, logger, regionService, httpResponseMessageHandler);

            await Assert.ThrowsAnyAsync<RedirectException>(async () => await service.PostContent("http://someUrl", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false)).ConfigureAwait(false);

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

            var service = new ContentRetriever(postHttpClient, logger, regionService, httpResponseMessageHandler);

            var result = await service.PostContent("http://someUrl", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

            Assert.Equal(DummyChildAppContent, result);

            httpResponseMessage.Dispose();
            fakeRedirectHttpMessageHandler.Dispose();
            postHttpClient.Dispose();
        }

        [Fact]
        public async Task PostContentWhenRegionIsNotHealthyReturnOfflineHTML()
        {
            const string offlineHTML = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = false,
                OfflineHTML = offlineHTML,
            };

            var result = await defaultService.PostContent("http://someUrl", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

            Assert.Equal(offlineHTML, result);
        }

        [Fact]
        public async Task PostContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsRequiredThenRegionStateUpdated()
        {
            var model = new RegionModel
            {
                IsHealthy = true,
                HeathCheckRequired = true,
            };

            var fakeLogger = A.Fake<ILogger<ContentRetriever>>();

            A.CallTo(() => fakeLogger.Log(LogLevel.Information, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IRegionService>();

            var service = new ContentRetriever(httpClient, fakeLogger, fakeRegionService, httpResponseMessageHandler);

            await service.PostContent("http://someUrl", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task PostContentWhenBrokenCircuitExceptionThrownAndHealthCheckIsNotRequiredThenReturnOfflineHTML()
        {
            const string offlineHTML = "<p>Offline HTML</p>";
            var model = new RegionModel
            {
                IsHealthy = true,
                OfflineHTML = offlineHTML,
            };

            var fakeLogger = A.Fake<ILogger<ContentRetriever>>();

            A.CallTo(() => fakeLogger.Log(LogLevel.Information, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored))
                .Throws<BrokenCircuitException>();

            var fakeRegionService = A.Fake<IRegionService>();

            var service = new ContentRetriever(httpClient, fakeLogger, fakeRegionService, httpResponseMessageHandler);

            var result = await service.PostContent("http://someUrl", model, defaultFormPostParams, "http://baseUrl").ConfigureAwait(false);

            A.CallTo(() => fakeRegionService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, false))
                .MustNotHaveHappened();

            Assert.Equal(offlineHTML, result);
        }
    }
}