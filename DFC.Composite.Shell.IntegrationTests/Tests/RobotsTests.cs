using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class RobotsTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public RobotsTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task Should_ReturnValidContent()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync(new Uri("/robots.txt", UriKind.Relative)).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.True(responseHtml.Contains("User-agent:", StringComparison.OrdinalIgnoreCase) || responseHtml.Contains("Disallow:", StringComparison.OrdinalIgnoreCase));
        }
    }
}