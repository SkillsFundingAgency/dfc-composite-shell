using DFC.Composite.Shell.Models.Health;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Test.ClientHandlers;
using DFC.Composite.Shell.UnitTests.HttpClientService;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationHealthServiceTests
    {
        private readonly ILogger<ApplicationHealthService> logger;
        private readonly HttpClient defaultHttpClient;

        public ApplicationHealthServiceTests()
        {
            logger = A.Fake<ILogger<ApplicationHealthService>>();
            defaultHttpClient = new HttpClient();
        }

        [Fact]
        public async Task GetAsyncReturnsHealthTextWhenApiReturnsHealthText()
        {
            // Arrange
            var expectedResponse = new ApplicationHealthModel
            {
                BearerToken = "SomeBearerToken",
                Data = new List<HealthItemModel>
                {
                    new HealthItemModel
                    {
                        Service = "A service 1",
                        Message = "A message 1",
                    },
                    new HealthItemModel
                    {
                        Service = "A service 2",
                        Message = "A message 2",
                    },
                },
            };

            var httpResponseBody = new List<HealthItemModel>
            {
                new HealthItemModel
                {
                    Service = "A service 1",
                    Message = "A message 1",
                },
                new HealthItemModel
                {
                    Service = "A service 2",
                    Message = "A message 2",
                },
            };

            var httpResponseString = JsonConvert.SerializeObject(httpResponseBody);
            var expectedResponseString = JsonConvert.SerializeObject(expectedResponse);

            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(httpResponseString),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyUrl") };

            var healthService = new ApplicationHealthService(httpClient, logger);
            var model = new ApplicationHealthModel { BearerToken = "SomeBearerToken" };

            // Act
            var result = await healthService.EnrichAsync(model);

            // Assert
            var resultString = JsonConvert.SerializeObject(result);
            Assert.Equal(expectedResponseString, resultString);
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            // Arrange
            var healthService = new ApplicationHealthService(defaultHttpClient, logger);

            // Act
            var result = await healthService.EnrichAsync(null);

            // Assert
            Assert.Null(result);
        }
    }
}
