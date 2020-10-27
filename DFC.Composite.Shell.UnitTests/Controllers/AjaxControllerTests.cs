using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models.AjaxApiModels;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AjaxRequest;
using DFC.Composite.Shell.Services.AppRegistry;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class AjaxControllerTests
    {
        private const string ValidPathName = "a-name";
        private const string ValidMethodName1 = "a-method1";
        private const string ValidMethodName2 = "a-method2";

        private readonly IAjaxRequestService fakeAjaxRequestService = A.Fake<IAjaxRequestService>();
        private readonly IAppRegistryDataService fakeAppRegistryDataService = A.Fake<IAppRegistryDataService>();

        [Fact]
        public async Task AjaxControllerActionActionReturnsSuccess()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
            var requestModel = ValidRequestModel();
            var appRegistrationModel = ValidAppRegistrationModel();
            var dummyResponseModel = A.Dummy<ResponseModel>();
            dummyResponseModel.Status = expectedStatusCode;

            A.CallTo(() => fakeAppRegistryDataService.GetAppRegistrationModel(A<string>.Ignored)).Returns(appRegistrationModel);
            A.CallTo(() => fakeAjaxRequestService.GetResponseAsync(A<RequestModel>.Ignored, A<AjaxRequestModel>.Ignored)).Returns(dummyResponseModel);

            var ajaxController = new AjaxController(fakeAjaxRequestService, fakeAppRegistryDataService);

            // Act
            var result = await ajaxController.Action(requestModel).ConfigureAwait(false);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.Equal(expectedStatusCode, (HttpStatusCode)objectResult.StatusCode);
        }

        [Fact]
        public async Task AjaxControllerActionActionReturnsBadRequestForMissingPathValue()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
            var requestModel = ValidRequestModel();
            requestModel.Path = null;
            var appRegistrationModel = ValidAppRegistrationModel();
            var dummyResponseModel = A.Dummy<ResponseModel>();
            dummyResponseModel.Status = expectedStatusCode;

            A.CallTo(() => fakeAppRegistryDataService.GetAppRegistrationModel(A<string>.Ignored)).Returns(appRegistrationModel);
            A.CallTo(() => fakeAjaxRequestService.GetResponseAsync(A<RequestModel>.Ignored, A<AjaxRequestModel>.Ignored)).Returns(dummyResponseModel);

            var ajaxController = new AjaxController(fakeAjaxRequestService, fakeAppRegistryDataService);

            // Act
            var result = await ajaxController.Action(requestModel).ConfigureAwait(false);

            // Assert
            var objectResult = result as BadRequestResult;
            Assert.Equal(expectedStatusCode, (HttpStatusCode)objectResult.StatusCode);
        }

        [Fact]
        public async Task AjaxControllerActionActionReturnsBadRequestForMissingMethodValue()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
            var requestModel = ValidRequestModel();
            requestModel.Method = null;
            var appRegistrationModel = ValidAppRegistrationModel();
            var dummyResponseModel = A.Dummy<ResponseModel>();
            dummyResponseModel.Status = expectedStatusCode;

            A.CallTo(() => fakeAppRegistryDataService.GetAppRegistrationModel(A<string>.Ignored)).Returns(appRegistrationModel);
            A.CallTo(() => fakeAjaxRequestService.GetResponseAsync(A<RequestModel>.Ignored, A<AjaxRequestModel>.Ignored)).Returns(dummyResponseModel);

            var ajaxController = new AjaxController(fakeAjaxRequestService, fakeAppRegistryDataService);

            // Act
            var result = await ajaxController.Action(requestModel).ConfigureAwait(false);

            // Assert
            var objectResult = result as BadRequestResult;
            Assert.Equal(expectedStatusCode, (HttpStatusCode)objectResult.StatusCode);
        }

        [Fact]
        public async Task AjaxControllerActionActionReturnsNotFoundForMissingEndpoint()
        {
            // Arrange
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
            var requestModel = ValidRequestModel();
            var appRegistrationModel = ValidAppRegistrationModel();
            appRegistrationModel.AjaxRequests.First().AjaxEndpoint = null;
            var dummyResponseModel = A.Dummy<ResponseModel>();
            dummyResponseModel.Status = expectedStatusCode;

            A.CallTo(() => fakeAppRegistryDataService.GetAppRegistrationModel(A<string>.Ignored)).Returns(appRegistrationModel);
            A.CallTo(() => fakeAjaxRequestService.GetResponseAsync(A<RequestModel>.Ignored, A<AjaxRequestModel>.Ignored)).Returns(dummyResponseModel);

            var ajaxController = new AjaxController(fakeAjaxRequestService, fakeAppRegistryDataService);

            // Act
            var result = await ajaxController.Action(requestModel).ConfigureAwait(false);

            // Assert
            var objectResult = result as NotFoundResult;
            Assert.Equal(expectedStatusCode, (HttpStatusCode)objectResult.StatusCode);
        }

        private AppRegistrationModel ValidAppRegistrationModel()
        {
            return new AppRegistrationModel
            {
                Path = ValidPathName,
                AjaxRequests = new List<AjaxRequestModel>
                {
                    new AjaxRequestModel
                    {
                        Name = ValidMethodName1,
                        AjaxEndpoint = "http://www.somewhere.com/1",
                    },
                    new AjaxRequestModel
                    {
                        Name = ValidMethodName2,
                        AjaxEndpoint = "http://www.somewhere.com/2",
                    },
                },
            };
        }

        private RequestModel ValidRequestModel()
        {
            return new RequestModel
            {
                Path = ValidPathName,
                Method = ValidMethodName1,
                AppData = "{ data: \"some-data\" }",
            };
        }
    }
}