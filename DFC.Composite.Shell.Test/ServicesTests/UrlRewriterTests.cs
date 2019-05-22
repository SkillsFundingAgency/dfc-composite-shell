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
            _urlRewriter = new UrlRewriter(_pathLocator.Object);
        }

        [Fact]
        public void Should_RewriteRelativeUrls()
        {
            var path = "path1";
            var content = "<a href='edit/1'></a>";
            var processedContentExpected = $"<a href='{path}?route=edit/1'></a>";
            _pathLocator.Setup(x => x.GetPath()).Returns(path);

            var processedContentActual = _urlRewriter.Rewrite(content);

            processedContentActual.Should().Be(processedContentExpected);
        }

        [Fact]
        public void ShouldNot_RewriteAbsoluteUrls()
        {
            var path = "path1";
            var content = "<a href='http://www.google.com'></a>";
            var processedContentExpected = content;
            _pathLocator.Setup(x => x.GetPath()).Returns(path);

            var processedContentActual = _urlRewriter.Rewrite(content);

            processedContentActual.Should().Be(processedContentExpected);
        }
    }
}
