using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Neo4J;
using DFC.Composite.Shell.Test.ClientHandlers;
using DFC.Composite.Shell.UnitTests.LogHandler;

using FakeItEasy;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class Neo4JServiceTests
    {
        private readonly IOptions<Neo4JSettings> settings;
        private readonly HttpClient client;
        private readonly FakeHttpMessageHandler fakeHttpMessageHandler;
        private readonly IFakeHttpRequestSender fakeHttpRequestSender;
        private readonly FakeLogger<Neo4JService> logger;

        public Neo4JServiceTests()
        {
            settings = Options.Create(new Neo4JSettings
            {
                SendData = true,
            });

            fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            logger = A.Fake<FakeLogger<Neo4JService>>();

            fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            client = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };
        }

        [Fact]
        public void WhenSettingsNullThrowError()
        {
            Assert.Throws<ArgumentNullException>(() => new Neo4JService(null, client, logger));
        }

        [Fact]
        public async Task WhenRequestNullThenDoNotCallVisitService()
        {
            var service = new Neo4JService(settings, client, logger);
            await service.InsertNewRequest(null);
            A.CallTo(() => logger.Log(LogLevel.Warning, A<Exception>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task WhenInsertNewRequestFailsThenLogError()
        {
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Throws(new Exception());

            var service = new Neo4JService(settings, client, logger);
            await service.InsertNewRequest(A.Fake<HttpRequest>());
            A.CallTo(() => logger.Log(LogLevel.Warning, A<Exception>.Ignored, A<string>.Ignored))
                .MustHaveHappened(1, Times.Exactly);
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenInsertNewRequestThenPostToVisitApi()
        {
            var service = new Neo4JService(settings, client, logger);
            await service.InsertNewRequest(A.Fake<HttpRequest>());

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenInsertNewRequestAndSendDateIsFalseThenDoNotPostToVisitApi()
        {
            var options = Options.Create(new Neo4JSettings
            {
                SendData = false,
            });

            var service = new Neo4JService(options, client, logger);

            await service.InsertNewRequest(A.Fake<HttpRequest>());

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustNotHaveHappened();
        }
    }
}
