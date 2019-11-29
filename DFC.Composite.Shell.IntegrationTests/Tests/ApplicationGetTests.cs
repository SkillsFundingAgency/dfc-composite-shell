using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationGetTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private const string Path = "path1";
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationGetTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task When_ShellUrlIsEntryPoint_ItContainsResponseFromRegisteredRegions()
        {
            var shellUrl = Path;
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUrl).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Assert.Contains("GET, http://www.path1.com/path1/body, path1, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task When_ShellUrlStartsWithPath_ItContainsResponseFromRegisteredRegions()
        {
            var shellUrl = string.Concat(Path, "/edit");
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUrl).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("GET, http://www.path1.com/path1/head/edit, path1, Head", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GET, http://www.path1.com/path1/body/edit, path1, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task When_ShellUrlStartsWithPathAndContainsQueryString_ItContainsResponseFromRegisteredRegions()
        {
            var shellUrl = string.Concat(Path, "/edit?id=1");
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUrl).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("GET, http://www.path1.com/path1/head/edit?id=1, path1, Head", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GET, http://www.path1.com/path1/body/edit?id=1, path1, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}