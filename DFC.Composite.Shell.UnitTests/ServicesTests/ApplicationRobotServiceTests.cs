using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Test.ClientHandlers;
using DFC.Composite.Shell.UnitTests.HttpClientService;
using FakeItEasy;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationRobotServiceTests
    {
        private readonly HttpClient defaultHttpClient;

        public ApplicationRobotServiceTests()
        {
            defaultHttpClient = new HttpClient();
        }

        [Fact]
        public async Task GetAsyncReturnsRobotTextWhenApiReturnsRobotText()
        {
            const string ExpectedResponseText = "SomeResponseText";
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ExpectedResponseText),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyCDNUrl") };

            var robotService = new ApplicationRobotService(httpClient);
            var model = new ApplicationRobotModel { BearerToken = "SomeBearerToken" };

            var result = await robotService.EnrichAsync(model);
            Assert.Equal(ExpectedResponseText, result.Data);
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            var robotService = new ApplicationRobotService(defaultHttpClient);

            var result = await robotService.EnrichAsync(null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncReturnsExceptionIfNoRobotsTextFound()
        {
            var robotService = new ApplicationRobotService(defaultHttpClient);

            var model = new ApplicationRobotModel { BearerToken = "SomeBearerToken" };
            var exceptionResult = await Assert.ThrowsAsync<InvalidOperationException>(async () => await robotService.EnrichAsync(model));

            Assert.StartsWith("An invalid request URI was provided.", exceptionResult.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
