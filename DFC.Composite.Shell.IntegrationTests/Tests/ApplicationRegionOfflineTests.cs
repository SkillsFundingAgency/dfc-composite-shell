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
            // Arrange
            var shellUri = new Uri("path3", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            // Act
            var response = await client.GetAsync(shellUri);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Path3 is offline", responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task WhenAnRegionIsOfflineAndOtherRegionsAreOnlineContentIncludesOfflineRegionHtmlAndContentFromOnlineRegions()
        {
            // Arrange
            var shellUri = new Uri("path3", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            // Act
            var response = await client.GetAsync(shellUri);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Path3 is offline", responseHtml, StringComparison.OrdinalIgnoreCase);
            //TODO 2nd part of test
        }
    }
}
