using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Test.ClientHandlers;
using DFC.Composite.Shell.UnitTests.HttpClientService;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class AppRegistryServiceTests
    {
        private readonly List<AppRegistrationModel> appRegistrationModels;

        public AppRegistryServiceTests()
        {
            appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "SomeFakePath",
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            IsHealthy = false,
                        },
                    },
                    AjaxRequests = new List<AjaxRequestModel>
                    {
                        new AjaxRequestModel
                        {
                            Name = "a-valid-name",
                            IsHealthy = false,
                        },
                    },
                },
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "SecondFakePath",
                },
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "ThirdFakePath",
                },
            };
        }

        [Fact]
        public async Task GetPathsReturnsPathModelResults()
        {
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(List<AppRegistrationModel>), appRegistrationModels, new JsonMediaTypeFormatter()),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);
            var result = await appRegistryService.GetPaths();

            Assert.Equal(appRegistrationModels, result);
        }

        [Fact]
        public async Task SetRegionHealthStateSuccess()
        {
            // Arrange
            const bool expectedResult = true;
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);

            // Act
            var result = await appRegistryService.SetRegionHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().Regions.First().PageRegion, expectedResult);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SetRegionHealthStateCircuitBreakerException()
        {
            // Arrange
            const bool expectedResult = false;
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Throws<BrokenCircuitException>();

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);

            // Act
            var result = await appRegistryService.SetRegionHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().Regions.First().PageRegion, expectedResult);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SetRegionHealthStateThrowsException()
        {
            // Arrange
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);

            // Act & Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await appRegistryService.SetRegionHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().Regions.First().PageRegion, true));
        }

        [Fact]
        public async Task SetAjaxRequestHealthStateSuccess()
        {
            // Arrange
            const bool expectedResult = true;
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);

            // Act
            var result = await appRegistryService.SetAjaxRequestHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().AjaxRequests.First().Name, expectedResult);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SetAjaxRequestHealthStateCircuitBreakerException()
        {
            // Arrange
            const bool expectedResult = false;
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Throws<BrokenCircuitException>();

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);

            // Act
            var result = await appRegistryService.SetAjaxRequestHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().AjaxRequests.First().Name, expectedResult);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SetAjaxRequestHealthStateThrowsException()
        {
            // Arrange
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryRequestService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryRequestService(logger, httpClient);

            // Act & Assert
            await Assert.ThrowsAnyAsync<HttpRequestException>(async () => await appRegistryService.SetAjaxRequestHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().AjaxRequests.First().Name, true));
        }
    }
}
