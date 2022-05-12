using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class ApplicationControllerTests
    {
        private const string ChildAppPath = "childapppath";
        private const string BadChildAppPath = "badchildapppath";
        private const string ChildAppData = "childappdata";
        private const string BadChildAppData = "badchildappdata";

        private readonly ActionGetRequestModel childAppActionGetRequestModel;
        private readonly ApplicationController defaultGetController;
        private readonly ApplicationController defaultPostController;
        private readonly ApplicationController bearerTokenController;
        private readonly ApplicationController postBearerTokenController;
        private readonly IBaseUrlService defaultBaseUrlService;
        private readonly IConfiguration defaultConfiguration;
        private readonly IVersionedFiles defaultVersionedFiles;
        private readonly IApplicationService defaultApplicationService;
        private readonly ILogger<ApplicationController> defaultLogger;
        private readonly IAppRegistryDataService defaultAppRegistryDataService;
        private readonly ApplicationToPageModelMapper defaultMapper;
        private readonly ActionPostRequestModel defaultPostRequestViewModel;
        private readonly ApplicationModel defaultApplicationModel;

        public ApplicationControllerTests()
        {
            defaultAppRegistryDataService = A.Fake<IAppRegistryDataService>();
            defaultMapper = new ApplicationToPageModelMapper(defaultAppRegistryDataService);
            defaultLogger = A.Fake<ILogger<ApplicationController>>();
            defaultApplicationService = A.Fake<IApplicationService>();
            defaultVersionedFiles = A.Fake<IVersionedFiles>();
            defaultConfiguration = A.Fake<IConfiguration>();
            defaultBaseUrlService = A.Fake<IBaseUrlService>();

            defaultApplicationModel = new ApplicationModel
            {
                AppRegistrationModel = new AppRegistrationModel
                {
                    Path = ChildAppPath,
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            IsHealthy = true,
                            PageRegion = PageRegion.Body,
                            RegionEndpoint = "http://childApp/bodyRegion",
                        },
                    },
                },
            };
            defaultPostRequestViewModel = new ActionPostRequestModel
            {
                Path = ChildAppPath,
                Data = ChildAppData,
                FormCollection = new FormCollection(new Dictionary<string, StringValues>
                {
                    { "someKey", "someFormValue" },
                }),
            };
            childAppActionGetRequestModel = defaultPostRequestViewModel;
            A.CallTo(() => defaultApplicationService.GetApplicationAsync(childAppActionGetRequestModel)).Returns(defaultApplicationModel);

            var fakeHttpContext = new DefaultHttpContext { Request = { QueryString = QueryString.Create("test", "testvalue") } };

            defaultGetController = new ApplicationController(defaultMapper, defaultLogger, defaultApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = fakeHttpContext,
                },
            };

            defaultPostController = new ApplicationController(defaultMapper, defaultLogger, defaultApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        Request = { Method = "POST" },
                    },
                },
            };

            bearerTokenController = new ApplicationController(defaultMapper, defaultLogger, defaultApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("bearer", "test") }, "mock")) },
                },
            };

            postBearerTokenController = new ApplicationController(defaultMapper, defaultLogger, defaultApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("bearer", "test") }, "mock")) },
                },
            };
        }

        [Fact]
        public async Task ApplicationControllerGetActionReturnsSuccess()
        {
            var response = await defaultGetController.Action(childAppActionGetRequestModel);

            var viewResult = Assert.IsAssignableFrom<ViewResult>(response);
            var model = Assert.IsAssignableFrom<PageViewModelResponse>(viewResult.ViewData.Model);
            Assert.Equal(model.Path, ChildAppPath);
        }

        [Fact]
        public async Task ApplicationControllerGetActionReturnsRedirectWhenRedirectExceptionOccurs()
        {
            var requestModel = new ActionGetRequestModel { Path = ChildAppPath, Data = ChildAppData };
            var fakeApplicationService = A.Fake<IApplicationService>();
            A.CallTo(() => fakeApplicationService.GetMarkupAsync(A<ApplicationModel>.Ignored, A<PageViewModel>.Ignored, A<string>.Ignored, A<string>.Ignored, A<IHeaderDictionary>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(A<ActionGetRequestModel>.Ignored)).Returns(defaultApplicationModel);

            var context = new DefaultHttpContext();

            using var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = context,
                },
            };

            await applicationController.Action(requestModel);
            Assert.True(context.Response.StatusCode == (int)HttpStatusCode.Redirect);
        }

        [Fact(Skip = "Needs revisiting as part of DFC-11808")]
        public async Task ApplicationControllerGetActionAddsModelStateErrorWhenPathIsNull()
        {
            var requestModel = new ActionGetRequestModel { Path = BadChildAppPath, Data = BadChildAppData };
            var fakeApplicationService = A.Fake<IApplicationService>();
            A.CallTo(() => fakeApplicationService.GetMarkupAsync(A<ApplicationModel>.Ignored, A<PageViewModel>.Ignored, A<string>.Ignored, A<string>.Ignored, A<IHeaderDictionary>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(childAppActionGetRequestModel)).Returns(defaultApplicationModel);

            using var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            await applicationController.Action(requestModel);

            A.CallTo(() => defaultLogger.Log<ApplicationController>(A<LogLevel>.Ignored, A<EventId>.Ignored, A<ApplicationController>.Ignored, A<Exception>.Ignored, A<Func<ApplicationController, Exception, string>>.Ignored)).MustHaveHappened(3, Times.Exactly);
        }

        [Fact(Skip = "Needs revisiting as part of DFC-11808")]
        public async Task ApplicationControllerGetActionThrowsAndLogsRedirectExceptionWhenExceptionOccurs()
        {
            var requestModel = new ActionGetRequestModel { Path = ChildAppPath, Data = ChildAppData };
            var fakeApplicationService = A.Fake<IApplicationService>();
            A.CallTo(() => fakeApplicationService.GetMarkupAsync(A<ApplicationModel>.Ignored, A<PageViewModel>.Ignored, A<string>.Ignored, A<string>.Ignored, A<IHeaderDictionary>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(childAppActionGetRequestModel)).Returns(defaultApplicationModel);

            using var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            await applicationController.Action(requestModel);

            A.CallTo(() => defaultLogger.Log(LogLevel.Information, 0, A<IReadOnlyList<KeyValuePair<string, object>>>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappened(4, Times.Exactly);
        }

        [Fact]
        public async Task ApplicationControllerPostActionReturnsSuccess()
        {
            var response = await defaultPostController.Action(defaultPostRequestViewModel);

            var viewResult = Assert.IsAssignableFrom<ViewResult>(response);
            var model = Assert.IsAssignableFrom<PageViewModelResponse>(viewResult.ViewData.Model);
            Assert.Equal(model.Path, ChildAppPath);
        }

        [Fact(Skip = "Needs revisiting as part of DFC-11808")]
        public async Task ApplicationControllerPostActionAddsModelStateErrorWhenPathIsNull()
        {
            var fakeApplicationService = A.Fake<IApplicationService>();
            var childAppActionGetRequestModel = new ActionGetRequestModel { Path = BadChildAppPath, Data = BadChildAppData };
            A.CallTo(() => fakeApplicationService.PostMarkupAsync(A<ApplicationModel>.Ignored, A<IEnumerable<KeyValuePair<string, string>>>.Ignored, A<PageViewModel>.Ignored, A<string>.Ignored, A<IHeaderDictionary>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(childAppActionGetRequestModel)).Returns(null as ApplicationModel);

            using var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { Request = { Method = "POST" }, },
                },
            };

            await applicationController.Action(defaultPostRequestViewModel);

            A.CallTo(() => defaultLogger.Log(LogLevel.Information, 0, A<IReadOnlyList<KeyValuePair<string, object>>>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappened(3, Times.Exactly);
        }

        [Fact]
        public async Task ApplicationControllerPostActionThrowsAndLogsRedirectExceptionWhenExceptionOccurs()
        {
            var fakeApplicationService = A.Fake<IApplicationService>();
            A.CallTo(() => fakeApplicationService.PostMarkupAsync(A<ApplicationModel>.Ignored, A<IEnumerable<KeyValuePair<string, string>>>.Ignored, A<PageViewModel>.Ignored, A<string>.Ignored, A<IHeaderDictionary>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(childAppActionGetRequestModel)).Returns(defaultApplicationModel);

            using var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { Request = { Method = "POST" }, },
                },
            };

            var result = await applicationController.Action(defaultPostRequestViewModel);

            Assert.NotNull(result);
        }
    }
}