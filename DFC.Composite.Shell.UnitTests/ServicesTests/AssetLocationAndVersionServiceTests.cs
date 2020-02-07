using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Utilities;
using DFC.Composite.Shell.Test.ClientHandlers;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class AssetLocationAndVersionServiceTests
    {
        private const string TestCDNLocation = "http://SomeDummyCDNUrl";
        private const string TestLocalLocation = "SomeLocalLocation";

        private readonly IAsyncHelper asyncHelper;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<AssetLocationAndVersionService> logger;
        private readonly IFileInfoHelper fileInfoHelper;
        private readonly HttpClient defaultHttpClient;

        public AssetLocationAndVersionServiceTests()
        {
            asyncHelper = new AsyncHelper();
            hostingEnvironment = A.Fake<IHostingEnvironment>();
            logger = A.Fake<ILogger<AssetLocationAndVersionService>>();
            fileInfoHelper = A.Fake<IFileInfoHelper>();
            defaultHttpClient = new HttpClient();
        }

        [Fact]
        public void GetCdnAssetFileAndVersionReturnsPathWithCurrentDateWhenNoValidResponseFromCDN()
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway,
                Content = new MultipartContent(),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);

            var assetLocationAndVersionService = new AssetLocationAndVersionService(httpClient, asyncHelper, hostingEnvironment, logger, fileInfoHelper);

            var result = assetLocationAndVersionService.GetCdnAssetFileAndVersion(TestCDNLocation);
            var expectedFormattedDate = DateTime.Now.ToString("yyyyMMddHH", CultureInfo.InvariantCulture);
            var expectedResult = $"{TestCDNLocation}?{expectedFormattedDate}";

            Assert.Equal(expectedResult, result);

            httpResponse.Dispose();
            fakeHttpMessageHandler.Dispose();
            httpClient.Dispose();
        }

        [Fact]
        public void GetCdnAssetFileAndVersionReturnsPathWithHashCodeWhenValidResponseFromCDN()
        {
            const string testHeader = "test";

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new MultipartContent(),
            };
            httpResponse.Content.Headers.Add("content-md5", testHeader);

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler);

            var assetLocationAndVersionService = new AssetLocationAndVersionService(httpClient, asyncHelper, hostingEnvironment, logger, fileInfoHelper);

            var result = assetLocationAndVersionService.GetCdnAssetFileAndVersion(TestCDNLocation);

            var expectedResult = $"{TestCDNLocation}?{testHeader}";

            Assert.Equal(expectedResult, result);

            httpResponse.Dispose();
            fakeHttpMessageHandler.Dispose();
            httpClient.Dispose();
        }

        [Fact]
        public void GetCdnAssetFileAndVersionLogsErrorWhenExceptionThrown()
        {
            var assetLocationAndVersionService = new AssetLocationAndVersionService(defaultHttpClient, asyncHelper, hostingEnvironment, logger, fileInfoHelper);

            var result = assetLocationAndVersionService.GetCdnAssetFileAndVersion(TestCDNLocation);

            result.Should().Be($"{TestCDNLocation}?{DateTime.Now.ToString("yyyyMMddHH", CultureInfo.InvariantCulture)}");
        }

        [Fact]
        public void GetLocalAssetFileAndVersionReturnsLocationWhenFileDoesNotExist()
        {
            var assetLocationAndVersionService = new AssetLocationAndVersionService(defaultHttpClient, asyncHelper, hostingEnvironment, logger, fileInfoHelper);

            var result = assetLocationAndVersionService.GetLocalAssetFileAndVersion(TestLocalLocation);

            Assert.Equal($"/{TestLocalLocation}?", result);
        }

        [Fact]
        public void GetLocalAssetFileAndVersionReturnsLocationWhenFileDoesExist()
        {
            var fakeStream = new MemoryStream(Encoding.UTF8.GetBytes("SomeTestData"));
            var fakeMD5 = MD5.Create();
            var expectedHashCode = BitConverter.ToString(fakeMD5.ComputeHash(fakeStream)).Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);

            fakeStream.Position = 0;

            var fileExists = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileExists.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileExists.GetStream(A<string>.Ignored)).Returns(fakeStream);

            var assetLocationAndVersionService = new AssetLocationAndVersionService(defaultHttpClient, asyncHelper, hostingEnvironment, logger, fileExists);

            var result = assetLocationAndVersionService.GetLocalAssetFileAndVersion(TestLocalLocation);

            Assert.Equal($"/{TestLocalLocation}?{expectedHashCode}", result);
            fakeStream.Dispose();
            fakeMD5.Dispose();
        }
    }
}