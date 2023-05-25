using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationRobotServiceTests
    {
        private readonly ILogger<ApplicationRobotService> logger;
        private readonly HttpClient defaultHttpClient;

        public ApplicationRobotServiceTests()
        {
            logger = A.Fake<ILogger<ApplicationRobotService>>();
            defaultHttpClient = new HttpClient();
        }

        [Fact]
        public async Task GetAsyncReturnsRobotTextWhenApiReturnsRobotText()
        {
            const string ExpectedResponseText = "SomeResponseText";
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ExpectedResponseText),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyCDNUrl") };

            var robotService = new ApplicationRobotService(httpClient);
            var model = new ApplicationRobotModel { BearerToken = "SomeBearerToken" };

            var result = await robotService.GetAsync(model);
            Assert.Equal(ExpectedResponseText, result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            var robotService = new ApplicationRobotService(defaultHttpClient);

            var result = await robotService.GetAsync(null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncReturnsExceptionIfNoRobotsTextFound()
        {
            var robotService = new ApplicationRobotService(defaultHttpClient);

            var model = new ApplicationRobotModel { BearerToken = "SomeBearerToken" };
            var exceptionResult = await Assert.ThrowsAsync<InvalidOperationException>(async () => await robotService.GetAsync(model));

            Assert.StartsWith("An invalid request URI was provided.", exceptionResult.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}