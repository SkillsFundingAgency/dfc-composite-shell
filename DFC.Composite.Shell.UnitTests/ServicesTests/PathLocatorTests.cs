using DFC.Composite.Shell.Services.PathLocator;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class PathLocatorTests
    {
        private readonly ILogger<UrlPathLocator> logger;

        public PathLocatorTests()
        {
            logger = A.Fake<ILogger<UrlPathLocator>>();
        }

        [Fact]
        public void ShouldGetPathFromUrl()
        {
            var path = "/courses";
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext).Returns(context);

            var urlPathLocator = new UrlPathLocator(httpContextAccessor, logger);
            var actualPath = urlPathLocator.GetPath();

            actualPath.Should().Be("courses");
        }

        [Fact]
        public void ShouldGetPathFromUrlWhenPathContainsTrailingSlash()
        {
            var path = "/courses/";
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext).Returns(context);

            var urlPathLocator = new UrlPathLocator(httpContextAccessor, logger);
            var actualPath = urlPathLocator.GetPath();

            actualPath.Should().Be("courses");
        }
    }
}