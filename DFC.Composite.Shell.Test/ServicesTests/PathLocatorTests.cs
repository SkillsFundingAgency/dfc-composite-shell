using DFC.Composite.Shell.Services.PathLocator;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class PathLocatorTests
    {
        private readonly IPathLocator _urlPathLocator;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ILogger<UrlPathLocator>> _logger;

        public PathLocatorTests()
        {
            _logger = new Mock<ILogger<UrlPathLocator>>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _urlPathLocator = new UrlPathLocator(_httpContextAccessor.Object, _logger.Object);
        }

        [Fact]
        public void ShouldGetPathFromUrl()
        {
            var path = "/courses";
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            _httpContextAccessor.Setup(x => x.HttpContext).Returns(context);
            
            var actualPath = _urlPathLocator.GetPath();

            actualPath.Should().Be("courses");
        }
    }
}
