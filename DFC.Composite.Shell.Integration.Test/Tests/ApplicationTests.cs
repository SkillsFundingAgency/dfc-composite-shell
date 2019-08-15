using DFC.Composite.Shell.Integration.Test.Framework;
using DFC.Composite.Shell.Integration.Test.Services;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test.Tests
{
    public class ApplicationTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> _factory;

        public ApplicationTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            _factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task When_EntryPointIsRequestedForPath_ContentIsFetchedForAllRegions()
        {
            var path = "path1";
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IPathService, TestPathService>();
                    services.AddTransient<IRegionService, TestRegionService>();
                    services.AddTransient<IContentRetriever, TestContentRetriever>();
                });
            }).CreateClient();

            var response = await client.GetAsync(path);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Contains("GET, http://localhost/path1/head, path1, Head", responseHtml);
            Assert.Contains("GET, http://localhost/path1/body, path1, Body", responseHtml);
        }
    }
}