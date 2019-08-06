using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Services.PathLocator;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace DFC.Composite.Shell.Test.HttpResponseMessageHandlers
{
    public class CookieHttpResponseMessageHandlerTests
    {
        private IPathLocator pathLocator;
        private IHttpContextAccessor httpContextAccessor;
        private CookieHttpResponseMessageHandler cookieHttpResponseMessageHandler;

        public CookieHttpResponseMessageHandlerTests()
        {
            pathLocator = A.Fake<IPathLocator>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();

            cookieHttpResponseMessageHandler = new CookieHttpResponseMessageHandler(httpContextAccessor, pathLocator);
        }

        [Fact]
        public void CanCopyCookieValuesFromChildAppToShellWhenCookieHasSingleValue()
        {
            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            var childHttpResponseMessage = new HttpResponseMessage();
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, "v1=v1value");
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Single(setCookieHeader);
            Assert.StartsWith(path, setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CanCopyCookieValuesFromChildAppToShellWhenCookieHasMultipleValues()
        {
            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            var childHttpResponseMessage = new HttpResponseMessage();
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string>() { "v1=value1", "v2=value2" });
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Equal(2, setCookieHeader.Count);
            Assert.StartsWith(path, setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CanCopyCookieValuesCorrectly()
        {
            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            var childHttpResponseMessage = new HttpResponseMessage();
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string>() { "v1=value1", "v2=value2" });
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Equal(2, setCookieHeader.Count);
            Assert.Contains("value1", setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
            Assert.Contains("value2", setCookieHeader[1], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CookieValuesAreCopiedToHttpContextItems()
        {
            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            var childHttpResponseMessage = new HttpResponseMessage();
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string>() { "v1=value1", "v2=value2" });
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellContextItems = httpContextAccessor.HttpContext.Items;
            Assert.Equal(2, shellContextItems.Count);
            Assert.Contains("value1", shellContextItems["path1v1"].ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("value2", shellContextItems["path1v2"].ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
