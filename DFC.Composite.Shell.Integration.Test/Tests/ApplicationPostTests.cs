using DFC.Composite.Shell.Integration.Test.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
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
            var client = _factory.CreateClientWithWebHostBuilder();

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
    }
}