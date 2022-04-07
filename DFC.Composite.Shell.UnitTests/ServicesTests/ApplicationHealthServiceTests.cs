using DFC.Composite.Shell.Models.HealthModels;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;

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
        private readonly ILogger<ApplicationHealthService> defaultLogger = A.Fake<ILogger<ApplicationHealthService>>();
        private readonly HttpClient defaultHttpClient = new HttpClient();

        [Fact]
        public async Task GetAsyncReturnsHealthTextWhenApiReturnsHealthText()
        {
            // Arrange
            var expectedResponse = new List<HealthItemModel>
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
            string expectedResponseString = JsonConvert.SerializeObject(expectedResponse);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponseString),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyUrl") };

            var healthService = new ApplicationHealthService(httpClient, defaultLogger);
            var model = new ApplicationHealthModel { BearerToken = "SomeBearerToken" };

            // Act
            var result = await healthService.GetAsync(model);
            foreach (var v in result)
            {
                v.ResponseTime = 0;
            }

            // Assert
            string resultString = JsonConvert.SerializeObject(result);
            Assert.Equal(expectedResponseString, resultString);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            // Arrange
            var healthService = new ApplicationHealthService(defaultHttpClient, defaultLogger);

            // Act
            var result = await healthService.GetAsync(null);

            // Assert
            Assert.Null(result);
        }
    }
}