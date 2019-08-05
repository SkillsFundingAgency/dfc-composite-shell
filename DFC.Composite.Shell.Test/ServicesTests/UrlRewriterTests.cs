using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.Services.UrlRewriter;
using FluentAssertions;
using Moq;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class UrlRewriterTests
    {
        private readonly IUrlRewriterService urlRewriterService;
        private readonly Mock<IPathLocator> pathLocator;

        public UrlRewriterTests()
        {
            pathLocator = new Mock<IPathLocator>();
            urlRewriterService = new UrlRewriterService();
        }

        [Fact(Skip = "Waiting for routing to be agreed upon")]
        public void ShouldRewriteRelativeUrls()
        {
            const string RequestBaseUrl = "path1";
            var content = "<a href='edit/1'></a>";
            var processedContentExpected = $"<a href='{RequestBaseUrl}?route=edit/1'></a>";
            pathLocator.Setup(x => x.GetPath()).Returns(RequestBaseUrl);

            var processedContentActual = urlRewriterService.Rewrite(content, RequestBaseUrl, RequestBaseUrl);

            processedContentActual.Should().Be(processedContentExpected);
        }

        [Fact]
        public void ShouldNotRewriteAbsoluteUrls()
        {
            const string RequestBaseUrl = "http://www.google.com";
            var path = "path1";
            var content = $"<a href='{RequestBaseUrl}'></a>";
            var processedContentExpected = content;
            pathLocator.Setup(x => x.GetPath()).Returns(path);

            var processedContentActual = urlRewriterService.Rewrite(content, RequestBaseUrl, RequestBaseUrl);

            processedContentActual.Should().Be(processedContentExpected);
        }
    }
}
