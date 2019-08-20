using DFC.Composite.Shell.Services.BaseUrl;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class BaseUrlServiceTests
    {
        private readonly BaseUrlService service;

        public BaseUrlServiceTests()
        {
            service = new BaseUrlService();
        }

        [Fact]
        public void GetBaseUrlReturnsEmptyStringIfNoRequest()
        {
            var result = service.GetBaseUrl(null, null);

            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void GetBaseUrlReturnsFormattedUrl()
        {
            const string fakeHostName = "DummyHostName";
            const string fakeUrlHelperContent = "UrlHelperString";

            var httpRequest = A.Fake<HttpRequest>();
            httpRequest.Scheme = "http";
            httpRequest.Host = new HostString(fakeHostName);

            var urlHelper = A.Fake<IUrlHelper>();
            A.CallTo(() => urlHelper.Content(A<string>.Ignored)).Returns(fakeUrlHelperContent);

            var expected = $"{httpRequest.Scheme}://{fakeHostName}{fakeUrlHelperContent}";
            var result = service.GetBaseUrl(httpRequest, urlHelper);

            Assert.Equal(expected, result);
        }
    }
}