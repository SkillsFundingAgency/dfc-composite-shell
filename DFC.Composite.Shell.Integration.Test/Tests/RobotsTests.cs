using DFC.Composite.Shell.Integration.Test.Framework;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class RobotsTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> _factory;

        public RobotsTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            _factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task Should_ReturnValidContent()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/robots.txt");

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.True(responseHtml.Contains("User-agent:") || responseHtml.Contains("Disallow:"));
        }
    }
}
