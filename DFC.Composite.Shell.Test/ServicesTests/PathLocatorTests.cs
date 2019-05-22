using DFC.Composite.Shell.Services.PathLocator;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class PathLocatorTests
    {
        private readonly IPathLocator _urlPathLocator;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;

        public PathLocatorTests()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _urlPathLocator = new UrlPathLocator(_httpContextAccessor.Object);
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
