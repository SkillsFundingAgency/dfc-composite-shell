using DFC.Composite.Shell.Services.UrlRewriter;
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
            const string shellAppUrl = "ShellAppUrl";
            const string childAppUrl = "ChildApplicationRootUrl";

            var content = $"<a href='{childAppUrl}/edit/1'></a>";
            var processedContentExpected = $"<a href='{shellAppUrl}/edit/1'></a>";

            var result = urlRewriterService.Rewrite(content, shellAppUrl, childAppUrl);

            Assert.Equal(result, processedContentExpected);
        }
    }
}