using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Neo4J;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
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


        public Neo4JServiceTests()
        {
            settings = Options.Create(new Neo4JSettings
            {
                SendData = true,
            });

            fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();

            fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            client = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };
        }

        [Fact]
        public void WhenSettingsNullThrowError()
        {
            Assert.Throws<ArgumentNullException>(() => new Neo4JService(null, client));
        }

        [Fact]
        public async Task WhenRequestNullThrowError()
        {
            var service = new Neo4JService(settings, client);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.InsertNewRequest(null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenInsertNewRequestThenPostToVisitApi()
        {
            var service = new Neo4JService(settings, client);
            await service.InsertNewRequest(A.Fake<HttpRequest>()).ConfigureAwait(false);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenInsertNewRequestAndSendDateIsFalseThenDoNotPostToVisitApi()
        {
            var options = Options.Create(new Neo4JSettings
            {
                SendData = false,
            });

            var service = new Neo4JService(options, client);

            await service.InsertNewRequest(A.Fake<HttpRequest>()).ConfigureAwait(false);

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).MustNotHaveHappened();
        }
    }
}
