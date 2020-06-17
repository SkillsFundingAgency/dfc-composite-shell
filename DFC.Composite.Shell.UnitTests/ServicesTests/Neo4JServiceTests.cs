using DFC.Composite.Shell.Services.Neo4J;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class Neo4JServiceTests
    {
        private readonly IOptions<Neo4JSettings> settings;
        private readonly IServiceProvider provider;
        private IGraphDatabase graphDatabase;

        public Neo4JServiceTests()
        {
            settings = Options.Create(new Neo4JSettings
            {
                UserName = "username",
                Password = "password",
                Endpoint = "endpoint",
                SendData = true,
            });

            provider = A.Fake<IServiceProvider>();
            graphDatabase = A.Fake<IGraphDatabase>();
            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => provider.GetService(typeof(ICustomCommand))).Returns(new CustomCommand());

        }

        [Fact]
        public void WhenSettingsNullThrowError()
        {
            Assert.Throws<ArgumentNullException>(() => new Neo4JService(null, graphDatabase, provider));
        }

        [Fact]
        public async Task WhenRequestNullThrowError()
        {
            var service = new Neo4JService(settings, graphDatabase, provider);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.InsertNewRequest(null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenInsertNewRequestThenUpdateNeo4J()
        {
            var service = new Neo4JService(settings, graphDatabase, provider);
            await service.InsertNewRequest(A.Fake<HttpRequest>()).ConfigureAwait(false);
            A.CallTo(() => graphDatabase.Run(A<ICommand[]>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenInsertNewRequestAndSendDateIsFalseThenDoNotUpdateNeo4J()
        {
            var options = Options.Create(new Neo4JSettings
            {
                UserName = "username",
                Password = "password",
                Endpoint = "endpoint",
                SendData = false,
            });

            var service = new Neo4JService(options, graphDatabase, provider);

            await service.InsertNewRequest(A.Fake<HttpRequest>()).ConfigureAwait(false);

            A.CallTo( () => graphDatabase.Run(A<ICommand[]>.Ignored)).MustNotHaveHappened();
        }
    }
}
