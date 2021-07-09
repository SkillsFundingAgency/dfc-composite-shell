using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.CookieParsers;
using DFC.Composite.Shell.Services.DataProtectionProviders;
using DFC.Composite.Shell.Services.HeaderCount;
using DFC.Composite.Shell.Services.HeaderRenamer;
using DFC.Composite.Shell.Services.PathLocator;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace DFC.Composite.Shell.Test.HttpResponseMessageHandlers
{
    public class CookieHttpResponseMessageHandlerTests
    {
        private readonly IPathLocator pathLocator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ISetCookieParser setCookieParser;
        private readonly CookieHttpResponseMessageHandler cookieHttpResponseMessageHandler;
        private readonly IHeaderRenamerService headerRenamerService;
        private readonly IHeaderCountService headerCountService;
        private readonly ICompositeDataProtectionDataProvider compositeDataProtectionDataProvider;

        public CookieHttpResponseMessageHandlerTests()
        {
            pathLocator = A.Fake<IPathLocator>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();
            setCookieParser = new SetCookieParser();
            httpContextAccessor.HttpContext = new DefaultHttpContext();
            headerRenamerService = new HeaderRenamerService();
            headerCountService = new HeaderCountService();
            compositeDataProtectionDataProvider = A.Fake<ICompositeDataProtectionDataProvider>();

            cookieHttpResponseMessageHandler = new CookieHttpResponseMessageHandler(httpContextAccessor, pathLocator, setCookieParser, headerRenamerService, headerCountService, compositeDataProtectionDataProvider);
        }

        [Fact]
        public void CanCopyCookieValuesFromChildAppToShellWhenCookieHasSingleValue()
        {
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
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
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string> { "v1=value1", "v2=value2" });
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
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string> { "v1=value1", "v2=value2" });
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Equal(2, setCookieHeader.Count);
            Assert.Contains("path1v1=value1", setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
            Assert.Contains("path1v2=value2", setCookieHeader[1], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CookieValuesAreCopiedToHttpContextItems()
        {
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string> { "v1=value1", "v2=value2" });
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellContextItems = httpContextAccessor.HttpContext.Items;
            Assert.Equal(2, shellContextItems.Count);
            Assert.Contains(shellContextItems, healthItem => healthItem.Key.ToString() == "path1v1");
            Assert.Contains(shellContextItems, healthItem => healthItem.Key.ToString() == "path1v2");
        }

        [Fact]
        public void HeaderWithNameDfcSessionIsNotPrefixedWithPath()
        {
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var path = "path1";
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string> { $"{Constants.DfcSession}=value1", "v2=value2" });
            childHttpResponseMessage.Headers.Add(HeaderNames.Referer, "Referer1=Referer1Value");

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Equal(2, setCookieHeader.Count);
            Assert.StartsWith($"{Constants.DfcSession}=value1", setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith($"{path}v2=value2", setCookieHeader[1], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void WhenMultipleDfcSessionsExistsFirstIsUsed()
        {
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString());
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string> { $"{Constants.DfcSession}=dfc1", "v2=value2", $"{Constants.DfcSession}=dfc2" });

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Equal(2, setCookieHeader.Count);
            Assert.StartsWith($"{Constants.DfcSession}=dfc1", setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith($"{path}v2=value2", setCookieHeader[1], StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void DfcSessionValueIsProtected()
        {
            using var childHttpResponseMessage = new HttpResponseMessage();

            //Arrange
            var suffix = "_abc";
            var path = "path1";
            A.CallTo(() => pathLocator.GetPath()).Returns(path);
            A.CallTo(() => compositeDataProtectionDataProvider.Protect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString() + suffix);
            A.CallTo(() => compositeDataProtectionDataProvider.Unprotect(A<string>.Ignored)).ReturnsLazily(x => x.Arguments.First().ToString().Substring(0, x.Arguments.First().ToString().Length - suffix.Length));
            childHttpResponseMessage.Headers.Add(HeaderNames.SetCookie, new List<string> { $"{Constants.DfcSession}=dfc1", "v2=value2", $"{Constants.DfcSession}=dfc2" });

            //Act
            cookieHttpResponseMessageHandler.Process(childHttpResponseMessage);

            //Assert
            var shellResponseHeaders = httpContextAccessor.HttpContext.Response.Headers;
            var setCookieHeader = shellResponseHeaders[HeaderNames.SetCookie];
            Assert.Equal(2, setCookieHeader.Count);
            Assert.StartsWith($"{Constants.DfcSession}=dfc1{suffix}", setCookieHeader[0], StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith($"{path}v2=value2", setCookieHeader[1], StringComparison.OrdinalIgnoreCase);
        }
    }
}
