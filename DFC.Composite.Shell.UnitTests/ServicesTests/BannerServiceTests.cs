using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.Banner;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;

using FakeItEasy;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class BannerServiceTests
    {
        private readonly HttpClient httpClient;
        private readonly BannerService bannerService;
        private readonly ILogger<BannerService> logger;
        private readonly IFakeHttpRequestSender fakeHttpRequestSender;
        private readonly FakeHttpMessageHandler fakeHttpMessageHandler;
        private HttpResponseMessage httpResponse;

        public BannerServiceTests()
        {
            logger = A.Fake<ILogger<BannerService>>();
            httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("some response."),
            };

            fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://bannerappurl"),
            };

            bannerService = new BannerService(httpClient, logger);
        }

        ~BannerServiceTests()
        {
            fakeHttpMessageHandler.Dispose();
            httpResponse.Dispose();
            httpClient.Dispose();
        }

        [Fact]
        public async Task GetPageBannersAsyncWhenBannersRetrievedThenContentStringReturned()
        {
            // Arrange
            var path = "somepath";

            // Act
            var result = await bannerService.GetPageBannersAsync(path);

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().Be("some response.");
        }

        [Fact]
        public async Task GetPageBannersAsyncWhenErrorCallingBannerAppThenThrowException()
        {
            // Arrange
            var expectedError = "some reason";
            var expectedStatusCode = HttpStatusCode.BadRequest;
            httpResponse = new HttpResponseMessage { StatusCode = expectedStatusCode, ReasonPhrase = expectedError };
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<EnhancedHttpException>(() => bannerService.GetPageBannersAsync(string.Empty));

            ex.Should().NotBeNull();
            ex.StatusCode.Should().Be(expectedStatusCode);
            ex.Message.Should().Be(expectedError);
        }
    }
}
