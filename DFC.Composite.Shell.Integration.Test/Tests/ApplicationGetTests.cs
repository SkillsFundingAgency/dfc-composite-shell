using DFC.Composite.Shell.Integration.Test.Framework;
using DFC.Composite.Shell.Integration.Test.Services;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test.Tests
{
    public class ApplicationGetTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> _factory;

        public ApplicationGetTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            _factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task When_ShellUrlIsEntryPoint_ItContainsResponseFromRegisteredRegions()
        {
            var path = "path1";
            var shellUrl = path;
            var client = CreateClient();

            var response = await client.GetAsync(shellUrl);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Contains("GET, http://www.path1.com/path1/head, path1, Head", responseHtml);
            Assert.Contains("GET, http://www.path1.com/path1/body, path1, Body", responseHtml);
        }

        [Fact]
        public async Task When_ShellUrlStartsWithPath_ItContainsResponseFromRegisteredRegions()
        {
            var path = "path1";
            var shellUrl = string.Concat(path, "/edit");
            var client = CreateClient();

            var response = await client.GetAsync(shellUrl);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Contains("GET, http://www.path1.com/path1/head/edit, path1, Head", responseHtml);
            Assert.Contains("GET, http://www.path1.com/path1/body/edit, path1, Body", responseHtml);
        }

        [Fact]
        public async Task When_ShellUrlStartsWithPathAndContainsQueryString_ItContainsResponseFromRegisteredRegions()
        {
            var path = "path1";
            var shellUrl = string.Concat(path, "/edit?id=1");
            var client = CreateClient();

            var response = await client.GetAsync(shellUrl);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Contains("GET, http://www.path1.com/path1/head/edit?id=1, path1, Head", responseHtml);
            Assert.Contains("GET, http://www.path1.com/path1/body/edit?id=1, path1, Body", responseHtml);
        }

        private HttpClient CreateClient()
        {
            return _factory.WithWebHostBuilder(x => RegisterService(x)).CreateClient();
        }

        private void RegisterService(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.ConfigureTestServices(services =>
            {
                services.AddTransient<IPathService, TestPathService>();
                services.AddTransient<IRegionService, TestRegionService>();
                services.AddTransient<IContentRetriever, TestContentRetriever>();
            });
        }
    }
}