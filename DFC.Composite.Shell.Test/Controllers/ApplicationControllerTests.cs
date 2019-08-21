using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Exceptions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class ApplicationControllerTests
    {
        private const string ChildAppPath = "ChildAppPath";

        private readonly ApplicationController defaultGetController;
        private readonly ApplicationController defaultPostController;
        private readonly IBaseUrlService defaultBaseUrlService;
        private readonly IConfiguration defaultConfiguration;
        private readonly IVersionedFiles defaultVersionedFiles;
        private readonly IApplicationService defaultApplicationService;
        private readonly ILogger<ApplicationController> defaultLogger;
        private readonly ApplicationToPageModelMapper defaultMapper;
        private readonly ActionPostRequestModel defaultPostRequestViewModel;
        private readonly ApplicationModel defaultApplicationModel;

        public ApplicationControllerTests()
        {
            defaultMapper = new ApplicationToPageModelMapper();
            defaultLogger = A.Fake<ILogger<ApplicationController>>();
            defaultApplicationService = A.Fake<IApplicationService>();
            defaultVersionedFiles = A.Fake<IVersionedFiles>();
            defaultConfiguration = A.Fake<IConfiguration>();
            defaultBaseUrlService = A.Fake<IBaseUrlService>();

            defaultApplicationModel = new ApplicationModel
            {
                Path = new PathModel { Path = ChildAppPath },
                Regions = new List<RegionModel>
                {
                    new RegionModel
                    {
                        Path = ChildAppPath,
                        IsHealthy = true,
                        PageRegion = PageRegion.Body,
                        RegionEndpoint = "http://childApp/bodyRegion",
                    },
                },
            };
            A.CallTo(() => defaultApplicationService.GetApplicationAsync(ChildAppPath)).Returns(defaultApplicationModel);

            var fakeHttpContext = new DefaultHttpContext { Request = { QueryString = QueryString.Create("test", "testvalue") } };

            defaultPostRequestViewModel = new ActionPostRequestModel
            {
                Path = ChildAppPath,
                FormCollection = new FormCollection(new Dictionary<string, StringValues>
                {
                    { "someKey", "someFormValue" },
                }),
            };

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
        }

        [Fact]
        public async Task ApplicationControllerGetActionReturnsSuccess()
        {
            var requestModel = new ActionGetRequestModel { Path = ChildAppPath };

            var response = await defaultGetController.Action(requestModel).ConfigureAwait(false);

            var viewResult = Assert.IsAssignableFrom<ViewResult>(response);
            var model = Assert.IsAssignableFrom<PageViewModelResponse>(viewResult.ViewData.Model);
            Assert.Equal(model.Path, ChildAppPath);
        }

        [Fact]
        public async Task ApplicationControllerGetActionAddsModelStateErrorWhenPathIsNull()
        {
            var requestModel = new ActionGetRequestModel { Path = null };

            var response = await defaultGetController.Action(requestModel).ConfigureAwait(false);

            var viewResult = Assert.IsAssignableFrom<ViewResult>(response);
            var errors = Assert.IsAssignableFrom<ModelStateDictionary>(viewResult.ViewData.ModelState);

            Assert.True(errors.ErrorCount > 0);
        }

        [Fact]
        public async Task ApplicationControllerGetActionThrowsAndLogsExceptionWhenExceptionOccurs()
        {
            var requestModel = new ActionGetRequestModel { Path = ChildAppPath };
            var applicationController = new ApplicationController(defaultMapper, defaultLogger, defaultApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = A.Fake<HttpContext>(),
                },
            };

            await Assert.ThrowsAnyAsync<Exception>(async () => await applicationController.Action(requestModel).ConfigureAwait(false)).ConfigureAwait(false);
            A.CallTo(() => defaultLogger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
            applicationController.Dispose();
        }

        [Fact]
        public async Task ApplicationControllerGetActionThrowsAndLogsRedirectExceptionWhenExceptionOccurs()
        {
            var requestModel = new ActionGetRequestModel { Path = ChildAppPath };
            var fakeApplicationService = A.Fake<IApplicationService>();
            A.CallTo(() => fakeApplicationService.GetMarkupAsync(A<ApplicationModel>.Ignored, A<string>.Ignored, A<PageViewModel>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(ChildAppPath)).Returns(defaultApplicationModel);

            var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            await applicationController.Action(requestModel).ConfigureAwait(false);

            A.CallTo(() => defaultLogger.Log(LogLevel.Information, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappened(2, Times.Exactly);
            applicationController.Dispose();
        }

        [Fact]
        public async Task ApplicationControllerPostActionReturnsSuccess()
        {
            var response = await defaultPostController.Action(defaultPostRequestViewModel).ConfigureAwait(false);

            var viewResult = Assert.IsAssignableFrom<ViewResult>(response);
            var model = Assert.IsAssignableFrom<PageViewModelResponse>(viewResult.ViewData.Model);
            Assert.Equal(model.Path, ChildAppPath);
        }

        [Fact]
        public async Task ApplicationControllerPostActionAddsModelStateErrorWhenPathIsNull()
        {
            var requestModel = new ActionPostRequestModel
            {
                Path = null,
                FormCollection = new FormCollection(new Dictionary<string, StringValues>
                {
                    { "someKey", "someFormValue" },
                }),
            };

            var response = await defaultPostController.Action(requestModel).ConfigureAwait(false);

            var viewResult = Assert.IsAssignableFrom<ViewResult>(response);
            var errors = Assert.IsAssignableFrom<ModelStateDictionary>(viewResult.ViewData.ModelState);

            Assert.True(errors.ErrorCount > 0);
        }

        [Fact]
        public async Task ApplicationControllerPostActionThrowsAndLogsExceptionWhenExceptionOccurs()
        {
            var fakehttpContext = A.Fake<HttpContext>();
            fakehttpContext.Request.Method = "POST";

            var applicationController = new ApplicationController(defaultMapper, defaultLogger, defaultApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = fakehttpContext,
                },
            };

            await Assert.ThrowsAnyAsync<Exception>(async () => await applicationController.Action(defaultPostRequestViewModel).ConfigureAwait(false)).ConfigureAwait(false);
            A.CallTo(() => defaultLogger.Log(LogLevel.Error, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappenedOnceExactly();
            applicationController.Dispose();
        }

        [Fact]
        public async Task ApplicationControllerPostActionThrowsAndLogsRedirectExceptionWhenExceptionOccurs()
        {
            var fakeApplicationService = A.Fake<IApplicationService>();
            A.CallTo(() => fakeApplicationService.PostMarkupAsync(A<ApplicationModel>.Ignored, A<string>.Ignored, A<string>.Ignored, A<IEnumerable<KeyValuePair<string, string>>>.Ignored, A<PageViewModel>.Ignored)).Throws<RedirectException>();
            A.CallTo(() => fakeApplicationService.GetApplicationAsync(ChildAppPath)).Returns(defaultApplicationModel);

            var applicationController = new ApplicationController(defaultMapper, defaultLogger, fakeApplicationService, defaultVersionedFiles, defaultConfiguration, defaultBaseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { Request = { Method = "POST" }, },
                },
            };

            await applicationController.Action(defaultPostRequestViewModel).ConfigureAwait(false);

            A.CallTo(() => defaultLogger.Log(LogLevel.Information, 0, A<FormattedLogValues>.Ignored, A<Exception>.Ignored, A<Func<object, Exception, string>>.Ignored)).MustHaveHappened(2, Times.Exactly);
            applicationController.Dispose();
        }
    }
}