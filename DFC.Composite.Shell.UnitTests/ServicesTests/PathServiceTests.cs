using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
            var responseContent = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "SomeFakePath",
                },
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(List<AppRegistrationModel>), responseContent, new JsonMediaTypeFormatter()),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            var logger = A.Fake<ILogger<AppRegistryService>>();
            var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var appRegistryService = new AppRegistryService(logger, httpClient);
            var result = await appRegistryService.GetPaths().ConfigureAwait(false);

            Assert.Equal(responseContent, result);

            httpResponse.Dispose();
            httpClient.Dispose();
            fakeHttpMessageHandler.Dispose();
        }
    }
}