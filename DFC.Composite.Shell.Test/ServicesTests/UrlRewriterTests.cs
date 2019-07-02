using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.Services.UrlRewriter;
using FluentAssertions;
using Moq;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class UrlRewriterTests
    {
        private readonly IUrlRewriter _urlRewriter;
        private readonly Mock<IPathLocator> _pathLocator;

        public UrlRewriterTests()
        {
            _pathLocator = new Mock<IPathLocator>();
            _urlRewriter = new UrlRewriter();
        }

        [Fact(Skip = "Waiting for routing to be agreed upon")]
        public void Should_RewriteRelativeUrls()
        {
            const string RequestBaseUrl = "path1";
            var content = "<a href='edit/1'></a>";
            var processedContentExpected = $"<a href='{RequestBaseUrl}?route=edit/1'></a>";
            _pathLocator.Setup(x => x.GetPath()).Returns(RequestBaseUrl);

            var processedContentActual = _urlRewriter.Rewrite(content, RequestBaseUrl, RequestBaseUrl);

            processedContentActual.Should().Be(processedContentExpected);
        }

        [Fact]
        public void ShouldNot_RewriteAbsoluteUrls()
        {
            const string RequestBaseUrl = "http://www.google.com";
            var path = "path1";
            var content = $"<a href='{RequestBaseUrl}'></a>";
            var processedContentExpected = content;
            _pathLocator.Setup(x => x.GetPath()).Returns(path);

            var processedContentActual = _urlRewriter.Rewrite(content, RequestBaseUrl, RequestBaseUrl);

            processedContentActual.Should().Be(processedContentExpected);
        }
    }
}
