using DFC.Composite.Shell.Models.Sitemap;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Test.ClientHandlers;
using DFC.Composite.Shell.UnitTests.HttpClientService;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ApplicationSitemapServiceTests
    {
        private readonly ILogger<ApplicationSitemapService> defaultLogger = A.Fake<ILogger<ApplicationSitemapService>>();
        private readonly HttpClient defaultHttpClient;

        public ApplicationSitemapServiceTests()
        {
            defaultHttpClient = new HttpClient();
        }

        [Fact]
        public async Task GetAsyncReturnsSitemapTextWhenApiReturnsSitemapText()
        {
            const string DummySitemapLocation = "http://SomeSitemapLocation";
            var responseText = $"<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"><url><loc>{DummySitemapLocation}</loc><priority>1</priority></url></urlset>";

            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseText),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<ApplicationSitemapService>>();
            using var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyCDNUrl") };

            var sitemapService = new ApplicationSitemapService(logger, httpClient);
            var model = new ApplicationSitemapModel { BearerToken = "SomeBearerToken" };

            var result = await sitemapService.EnrichAsync(model);
            var resultLocations = result.Data.Select(location => location.Url);

            Assert.Contains(DummySitemapLocation, resultLocations);
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            var sitemapService = new ApplicationSitemapService(defaultLogger, defaultHttpClient);

            var result = await sitemapService.EnrichAsync(null);

            Assert.Null(result);
        }
    }
}
