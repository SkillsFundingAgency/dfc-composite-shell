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
            var shellUri = new Uri("path1", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUri).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("GET, http://www.path1.com/path1/body, path1, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}