using DFC.Composite.Shell.Integration.Test.Framework;
using DFC.Composite.Shell.Integration.Test.Services;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test.Tests
{
    public class ApplicationPostTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> _factory;

        public ApplicationPostTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            _factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task When_ShellSendsPostData_ItsSendToRegisteredApplication()
        {
            var path = "path1";
            var shellUrl = string.Concat(path, "/edit?id=1");
            var client = CreateClient();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("field1", "value1"),
                new KeyValuePair<string, string>("field2", "value2")
            });

            var response = await client.PostAsync(shellUrl, formContent);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Contains("POST, http://www.path1.com/path1/edit, path1, Body, field1=value1, field2=value2", responseHtml);
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