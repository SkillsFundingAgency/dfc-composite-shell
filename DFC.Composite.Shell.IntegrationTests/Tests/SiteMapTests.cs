using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class SiteMapTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public SiteMapTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task Should_ReturnValidContent()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/sitemap.xml").ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Equal(MediaTypeNames.Application.Xml, response.Content.Headers.ContentType.MediaType);
            Assert.True(responseHtml.Contains("<urlset", StringComparison.OrdinalIgnoreCase) && responseHtml.Contains("<url>", StringComparison.OrdinalIgnoreCase));
        }
    }
}