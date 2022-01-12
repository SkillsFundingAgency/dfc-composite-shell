using DFC.Composite.Shell.Models.AjaxApiModels;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AjaxRequest;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Polly.CircuitBreaker;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class AjaxRequestServiceTests
    {
        private const string ValidPathName = "a-name";
        private const string ValidMethodName = "a-method";

        private readonly ILogger<AjaxRequestService> fakeLogger = A.Fake<ILogger<AjaxRequestService>>();
        private readonly IAppRegistryDataService fakeAppRegistryDataService = A.Fake<IAppRegistryDataService>();

        [Fact]
        public async Task AjaxRequestServiceGetResponseReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            const bool expectedIsHealthy = true;
            var requestModel = ValidRequestModel();
            var ajaxRequest = ValidAjaxRequestModel(expectedIsHealthy);
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = expectedStatusCode,
                Content = new ObjectContent(typeof(string), "{ \"data\": \"some response data\" }", new JsonMediaTypeFormatter()),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var ajaxRequestService = new AjaxRequestService(fakeLogger, fakeAppRegistryDataService, httpClient);

            // Act
            var result = await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest);

            // Assert
            A.CallTo(() => fakeAppRegistryDataService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedStatusCode, result.Status);
            Assert.Equal(expectedIsHealthy, result.IsHealthy);
        }

        [Fact]
        public async Task AjaxRequestServiceGetResponseReturnsUnsuccessful()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
            const bool expectedIsHealthy = true;
            var requestModel = ValidRequestModel();
            var ajaxRequest = ValidAjaxRequestModel(expectedIsHealthy);
            using var httpResponse = new HttpResponseMessage
            {
                StatusCode = expectedStatusCode,
                Content = new ObjectContent(typeof(string), "{ \"data\": \"some response data\" }", new JsonMediaTypeFormatter()),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            var ajaxRequestService = new AjaxRequestService(fakeLogger, fakeAppRegistryDataService, httpClient);

            // Act
            var result = await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest);

            // Assert
            A.CallTo(() => fakeAppRegistryDataService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedStatusCode, result.Status);
            Assert.Equal(expectedIsHealthy, result.IsHealthy);
        }

        [Fact]
        public async Task AjaxRequestServiceGetResponseCatchesTaskCanceledException()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            const bool expectedIsHealthy = false;
            var requestModel = ValidRequestModel();
            var ajaxRequest = ValidAjaxRequestModel(true);
            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();

            using var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            using var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomePathBaseAddress"),
            };

            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Throws<TaskCanceledException>();

            var ajaxRequestService = new AjaxRequestService(fakeLogger, fakeAppRegistryDataService, httpClient);

            // Act
            var result = await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest);

            // Assert
            A.CallTo(() => fakeAppRegistryDataService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();

            Assert.Equal(expectedStatusCode, result.Status);
            Assert.Equal(expectedIsHealthy, result.IsHealthy);
        }

        [Fact]
        public async Task AjaxRequestServiceGetResponseReturnsSuccessWhenUnhealthy()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            const bool expectedIsHealthy = false;
            var requestModel = ValidRequestModel();
            var ajaxRequest = ValidAjaxRequestModel(expectedIsHealthy);
            using var httpClient = new HttpClient();

            var ajaxRequestService = new AjaxRequestService(fakeLogger, fakeAppRegistryDataService, httpClient);

            // Act
            var result = await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest);

            // Assert
            A.CallTo(() => fakeAppRegistryDataService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();

            Assert.Equal(expectedStatusCode, result.Status);
            Assert.Equal(expectedIsHealthy, result.IsHealthy);
        }

        [Fact]
        public async Task AjaxRequestServiceGetResponseTrowsExceptionForMissingRequestModel()
        {
            // Arrange
            const bool expectedIsHealthy = true;
            RequestModel requestModel = null;
            var ajaxRequest = ValidAjaxRequestModel(expectedIsHealthy);
            using var httpClient = new HttpClient();

            var ajaxRequestService = new AjaxRequestService(fakeLogger, fakeAppRegistryDataService, httpClient);

            // Act & Assert
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest));
        }

        [Fact]
        public async Task AjaxRequestServiceGetResponseTrowsExceptionForMissingAjaxRequestModel()
        {
            // Arrange
            var requestModel = ValidRequestModel();
            AjaxRequestModel ajaxRequest = null;
            using var httpClient = new HttpClient();

            var ajaxRequestService = new AjaxRequestService(fakeLogger, fakeAppRegistryDataService, httpClient);

            // Act & Assert
            await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest));
        }

        private RequestModel ValidRequestModel()
        {
            return new RequestModel
            {
                Path = ValidPathName,
                Method = ValidMethodName,
                AppData = "{ data: \"some-data\" }",
            };
        }

        private AjaxRequestModel ValidAjaxRequestModel(bool isHealthy)
        {
            return new AjaxRequestModel
            {
                AjaxEndpoint = "http://www.someehere.com/search/{0}/sort",
                IsHealthy = isHealthy,
                HealthCheckRequired = true,
                OfflineHtml = "<p>It is broken</p>",
            };
        }
    }
}
