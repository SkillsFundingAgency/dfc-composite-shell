using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Composite.Shell.IntegrationTests.Fakes;
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
            // Arrange
            var shellUri = new Uri("path1/edit?id=1", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();
            var expected = "POST, http://www.expected-domain.com/expected-path/edit/body, path1, Body, field1=value1, field2=value2";

            using var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("field1", "value1"),
                new KeyValuePair<string, string>("field2", "value2"),
            });

            // Act
            var response = await client.PostAsync(shellUri, formContent);
            response.EnsureSuccessStatusCode();
            var actual = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task When_ShellSendsPostData_ItsSendItToRegisteredApplicationAndReturnsFileDownload()
        {
            // Arrange
            var shellUri = new Uri("path1/edit?id=1", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            using var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("download", "true"),
            });

            // Act
            var response = await client.PostAsync(shellUri, formContent);
            response.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(FakeContentRetriever.FileContentType, response.Content.Headers.ContentType.MediaType);
            Assert.Equal(FakeContentRetriever.FileName, response.Content.Headers.ContentDisposition.FileNameStar);
        }

    }
}
