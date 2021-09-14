using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationOfflineTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationOfflineTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task WhenAnApplicationIsOfflineItContainsTheApplicationsOfflineMessage()
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
        public async Task WhenAnApplicationIsOfflineItDoesntContainsContentFromAnyRegions()
        {
            // Arrange
            var shellUri = new Uri("path3", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            // Act
            var response = await client.GetAsync(shellUri);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.DoesNotContain("GET, http://www.expected-domain.com/path3/head, path3, Head", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GET, http://www.expected-domain.com/path3/body, path3, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GET, http://www.expected-domain.com/path3/breadcrumb, path3, Breadcrumb", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GET, http://www.expected-domain.com/path3/bodyfooter, path3, Bodyfooter", responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}
