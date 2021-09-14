using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationOnlineTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationOnlineTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task WhenAnApplicationIsOnlineItContainsContentFromAllOnlineRegions()
        {
            // Arrange
            var shellUri = new Uri("pages", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();
            var expected = "GET, http://www.expected-domain.com/expected-path/body, pages, Body";

            // Act
            var response = await client.GetAsync(shellUri);
            response.EnsureSuccessStatusCode();

            var actual = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(expected, actual, StringComparison.OrdinalIgnoreCase);
        }
    }
}
