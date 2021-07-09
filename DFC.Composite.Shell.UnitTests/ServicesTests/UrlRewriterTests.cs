using DFC.Composite.Shell.Services.UrlRewriter;
using System;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class UrlRewriterTests
    {
        private readonly IUrlRewriterService urlRewriterService;

        public UrlRewriterTests()
        {
            urlRewriterService = new UrlRewriterService();
        }

        [Fact]
        public void ShouldRewriteChildApplicationUrls()
        {
            // Arrange
            var shellAppUrl = new Uri("http://ShellAppUrl");
            var childAppUrl = new Uri("http://ChildApplicationRootUrl");

            var content = $"<a href='{childAppUrl}/edit/1'></a>";
            var processedContentExpected = $"<a href='{shellAppUrl}/edit/1'></a>";

            // Act
            var result = urlRewriterService.RewriteAttributes(content, shellAppUrl, childAppUrl);

            // Assert
            Assert.Equal(result, processedContentExpected);
        }
    }
}
