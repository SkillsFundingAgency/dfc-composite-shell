using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationRegionOfflineTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationRegionOfflineTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task WhenAnRegionIsOfflineContentIncludesTheRegionsOfflineHtml()
        {
            var shellUri = new Uri("path1/article", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUri).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("path1 region bodyfooter is offline", responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task WhenAnRegionIsOfflineAndOtherRegionsAreOnlineContentIncludesOfflineRegionHtmlAndContentFromOnlineRegions()
        {
            var shellUri = new Uri("path1/article", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUri).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("path1 region bodyfooter is offline", responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}