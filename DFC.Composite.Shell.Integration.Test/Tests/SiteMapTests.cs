using DFC.Composite.Shell.Integration.Test.Framework;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test.Tests
{
    public class SiteMapTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> _factory;

        public SiteMapTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            _factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task Should_ReturnValidContent()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/sitemap.xml");

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();
            Assert.True(responseHtml.Contains("<urlset") && responseHtml.Contains("<url>"));
        }
    }
}
