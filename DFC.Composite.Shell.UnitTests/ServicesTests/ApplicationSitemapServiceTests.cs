using DFC.Composite.Shell.Models.SitemapModels;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
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
        private readonly ILogger<ApplicationSitemapService> logger;
        private readonly HttpClient defaultHttpClient;

        public ApplicationSitemapServiceTests()
        {
            logger = A.Fake<ILogger<ApplicationSitemapService>>();
            defaultHttpClient = new HttpClient();
        }

        [Fact]
        public async Task GetAsyncReturnsSitemapTextWhenApiReturnsSitemapText()
        {
            const string DummySitemapLocation = "http://SomeSitemapLocation";
            var responseText = $"<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"><url><loc>{DummySitemapLocation}</loc><priority>1</priority></url></urlset>";

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseText),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://SomeDummyCDNUrl") };

            var sitemapService = new ApplicationSitemapService(httpClient, logger);
            var model = new ApplicationSitemapModel { BearerToken = "SomeBearerToken" };

            var result = await sitemapService.GetAsync(model).ConfigureAwait(false);
            var resultLocations = result.Select(r => r.Url);

            Assert.Contains(DummySitemapLocation, resultLocations);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task GetAsyncReturnsNullIfModelIsNull()
        {
            var sitemapService = new ApplicationSitemapService(defaultHttpClient, logger);

            var result = await sitemapService.GetAsync(null).ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncReturnsExceptionIfNoSitemapsTextFound()
        {
            var sitemapService = new ApplicationSitemapService(defaultHttpClient, logger);

            var model = new ApplicationSitemapModel { BearerToken = "SomeBearerToken" };
            var exceptionResult = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sitemapService.GetAsync(model).ConfigureAwait(false)).ConfigureAwait(false);

            Assert.StartsWith("An invalid request URI was provided.", exceptionResult.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}