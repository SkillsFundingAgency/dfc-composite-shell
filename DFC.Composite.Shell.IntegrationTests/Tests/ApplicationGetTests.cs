using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationGetTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationGetTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task When_ShellUrlIsEntryPoint_ItContainsResponseFromRegisteredRegions()
        {
            // Arrange
            var shellUri = new Uri("path1", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            // Act
            var response = await client.GetAsync(shellUri);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("GET, http://www.expected-domain.com/expected-path/path1/body, pages, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task When_ShellUrlStartsWithPath_ItContainsResponseFromRegisteredRegions()
        {
            // Arrange
            var shellUri = new Uri("path1/edit", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();
            var expected1 = "GET, http://www.expected-domain.com/expected-path/edit/head, path1, Head";
            var expected2 = "GET, http://www.expected-domain.com/expected-path/edit/body, path1, Body";

            // Act
            var response = await client.GetAsync(shellUri);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(expected1, responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(expected2, responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task When_ShellUrlStartsWithPathAndContainsQueryString_ItContainsResponseFromRegisteredRegions()
        {
            // Arrange
            var shellUri = new Uri("path1/edit?id=1", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();
            var expected1 = "GET, http://www.expected-domain.com/expected-path/edit/head?id=1, path1, Head";
            var expected2 = "GET, http://www.expected-domain.com/expected-path/edit/body?id=1, path1, Body";

            // Act
            var response = await client.GetAsync(shellUri);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(expected1, responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(expected2, responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}
