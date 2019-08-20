using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class PathServiceTests
    {
        [Fact]
        public async Task GetPathsReturnsPathModelResults()
        {
            var responseContent = new List<PathModel>
            {
                new PathModel
                {
                    IsOnline = true,
                    Path = "SomeFakePath",
                },
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(List<PathModel>), responseContent, new JsonMediaTypeFormatter()),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var pathService = new PathService(httpClient);
            var result = await pathService.GetPaths().ConfigureAwait(false);

            Assert.Equal(responseContent, result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }
    }
}