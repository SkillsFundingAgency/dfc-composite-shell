using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
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
            const string ResponseText = "SomeResponseText";
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ResponseText),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyCDNUrl") };

            var robotService = new ApplicationRobotService(httpClient, logger);
            var model = new ApplicationRobotModel { BearerToken = "SomeBearerToken" };

            var result = await robotService.GetAsync(model).ConfigureAwait(false);
            Assert.Equal($"{ResponseText}\r\n", result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            var robotService = new ApplicationRobotService(defaultHttpClient, logger);

            var result = await robotService.GetAsync(null).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfNoRobotsTextFound()
        {
            var robotService = new ApplicationRobotService(defaultHttpClient, logger);

            var model = new ApplicationRobotModel { BearerToken = "SomeBearerToken" };
            var result = await robotService.GetAsync(model).ConfigureAwait(false);

            Assert.Null(result);
            A.CallTo(() => logger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
        }
    }
}