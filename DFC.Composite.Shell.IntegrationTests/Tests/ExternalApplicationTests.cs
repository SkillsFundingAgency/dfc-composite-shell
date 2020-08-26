using DFC.Composite.Shell.Integration.Test.Framework;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ExternalApplicationTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ExternalApplicationTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task CanRedirectToExternalUrl()
        {
            factory.ClientOptions.AllowAutoRedirect = false;
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(new Uri("/externalpath1", UriKind.Relative)).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("http://www.externalpath1.com/", response.Headers.Location.AbsoluteUri);
        }

        [Theory]
        [InlineData ("product; (product with ;)")]
        [InlineData("product (product with out ;)")]
        public async Task CanRedirectToExternalUrlWithUserAgent(string userAgent)
        {
            factory.ClientOptions.AllowAutoRedirect = false;
            var client = factory.CreateClientWithWebHostBuilder();

            var validUserAgent = client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, userAgent);

            var response = await client.GetAsync(new Uri("/externalpath1", UriKind.Relative)).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("http://www.externalpath1.com/", response.Headers.Location.AbsoluteUri);
        }
    }
}