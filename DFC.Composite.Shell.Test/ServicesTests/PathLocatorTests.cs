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
        private readonly IPathLocator urlPathLocator;
        private readonly Mock<IHttpContextAccessor> httpContextAccessor;
        private readonly Mock<ILogger<UrlPathLocator>> logger;

        public PathLocatorTests()
        {
            logger = new Mock<ILogger<UrlPathLocator>>();
            httpContextAccessor = new Mock<IHttpContextAccessor>();
            urlPathLocator = new UrlPathLocator(httpContextAccessor.Object, logger.Object);
        }

        [Fact]
        public void ShouldGetPathFromUrl()
        {
            var path = "/courses";
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            httpContextAccessor.Setup(x => x.HttpContext).Returns(context);
            var actualPath = urlPathLocator.GetPath();

            actualPath.Should().Be("courses");
        }
    }
}