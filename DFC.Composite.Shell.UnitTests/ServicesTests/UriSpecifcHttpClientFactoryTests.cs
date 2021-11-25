using DFC.Composite.Shell.Services.UriSpecificHttpClient;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class UriSpecifcHttpClientFactoryTests
    {
        private readonly UriSpecifcHttpClientFactory factory;
        private readonly IHttpClientFactory clientFactory;

        public UriSpecifcHttpClientFactoryTests()
        {
            var registeredUrls = A.Fake<IRegisteredUrls>();
            A.CallTo(() => registeredUrls.GetAll()).Returns(new List<RegisteredUrlModel> { new RegisteredUrlModel{ Url = "http://example.org/should-exist" } });

            clientFactory = A.Fake<IHttpClientFactory>();
            factory = new UriSpecifcHttpClientFactory(clientFactory, registeredUrls, A.Fake<ILogger<UriSpecifcHttpClientFactory>>());
        }

        [Fact]
        public void WhenUrlNotRegistedShouldGetDefault()
        {
            factory.GetClientForRegionEndpoint("http://example.org/doesnt-exist");

            A.CallTo(() => clientFactory.CreateClient("CATCH_ALL_REGISTERED_URL_KEY_UriSpecifcHttpClientFactory")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void WhenUrlIsRegistedShouldGetSpecific()
        {
            factory.GetClientForRegionEndpoint("http://example.org/should-exist");

            A.CallTo(() => clientFactory.CreateClient("http://example.org/should-exist_UriSpecifcHttpClientFactory")).MustHaveHappenedOnceExactly();
        }
    }
}