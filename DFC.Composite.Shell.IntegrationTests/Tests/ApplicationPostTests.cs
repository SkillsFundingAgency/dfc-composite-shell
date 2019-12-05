using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationPostTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationPostTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task When_ShellSendsPostData_ItsSendItToRegisteredApplication()
        {
            var path = "path1";
            var shellUri = new Uri(string.Concat(path, "/edit?id=1"), UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("field1", "value1"),
                new KeyValuePair<string, string>("field2", "value2"),
            });

            var response = await client.PostAsync(shellUri, formContent).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("POST, http://www.path1.com/path1/edit, path1, Body, field1=value1, field2=value2", responseHtml, StringComparison.OrdinalIgnoreCase);
            formContent.Dispose();
        }
    }
}