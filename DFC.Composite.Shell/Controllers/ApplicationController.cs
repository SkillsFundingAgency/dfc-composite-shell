using DFC.Composite.Shell.Exceptions;
using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class ApplicationController : Controller
    {
        private const string MainRenderViewName = "Application/RenderView";

        private readonly IMapper<ApplicationModel, PageViewModel> mapper;
        private readonly ILogger<ApplicationController> logger;
        private readonly IApplicationService applicationService;
        private readonly IVersionedFiles versionedFiles;
        private readonly IConfiguration configuration;
        private readonly IBaseUrlService baseUrlService;

        public ApplicationController(
            IMapper<ApplicationModel, PageViewModel> mapper,
            ILogger<ApplicationController> logger,
            IApplicationService applicationService,
            IVersionedFiles versionedFiles,
            IConfiguration configuration,
            IBaseUrlService baseUrlService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.applicationService = applicationService;
            this.versionedFiles = versionedFiles;
            this.configuration = configuration;
            this.baseUrlService = baseUrlService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(ActionGetRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            try
            {
                if (requestViewModel != null)
                {
                    logger.LogInformation($"{nameof(Action)}: Getting child response for: {requestViewModel.Path}");

                    var application = await applicationService.GetApplicationAsync(requestViewModel.Path)
                        .ConfigureAwait(false);

                    if (application?.Path == null)
                    {
                        var errorString = $"The path {requestViewModel.Path} is not registered";

                        ModelState.AddModelError(string.Empty, errorString);
                        logger.LogWarning($"{nameof(Action)}: {errorString}");
                    }
                    else
                    {
                        mapper.Map(application, viewModel);

                        applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                        await applicationService.GetMarkupAsync(application, requestViewModel.Data + Request.QueryString, viewModel).ConfigureAwait(false);

                        logger.LogInformation(
                            $"{nameof(Action)}: Received child response for: {requestViewModel.Path}");
                    }
                }
            }
            catch (RedirectException ex)
            {
                logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation?.PathAndQuery} to: {ex.Location?.PathAndQuery}");

                Response.Redirect(ex.Location?.PathAndQuery, ex.IsPermenant);
            }
            catch (Exception ex)
            {
                var errorString = $"{requestViewModel?.Path}: {ex.Message}";

                ModelState.AddModelError(string.Empty, errorString);
                logger.LogError(ex, $"{nameof(Action)}: Error getting child response for: {errorString}");
            }

            return View(MainRenderViewName, viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Action(ActionPostRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            try
            {
                if (requestViewModel != null)
                {
                    logger.LogInformation($"{nameof(Action)}: Posting child request for: {requestViewModel.Path}");

                    var application = await applicationService.GetApplicationAsync(requestViewModel.Path).ConfigureAwait(false);

                    if (application?.Path == null)
                    {
                        var errorString = $"The path {requestViewModel.Path} is not registered";

                        ModelState.AddModelError(string.Empty, errorString);
                        logger.LogWarning($"{nameof(Action)}: {errorString}");
                    }
                    else
                    {
                        mapper.Map(application, viewModel);

                        applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                        var formParameters = (from a in requestViewModel.FormCollection
                                              select new KeyValuePair<string, string>(a.Key, a.Value)).ToArray();

                        await applicationService.PostMarkupAsync(application, requestViewModel.Path, requestViewModel.Data, formParameters, viewModel).ConfigureAwait(false);

                        logger.LogInformation($"{nameof(Action)}: Received child response for: {requestViewModel.Path}");
                    }
                }
            }
            catch (RedirectException ex)
            {
                logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation?.PathAndQuery} to: {ex.Location?.PathAndQuery}");

                Response.Redirect(ex.Location?.PathAndQuery, true);
            }
            catch (Exception ex)
            {
                var errorString = $"{requestViewModel?.Path}: {ex.Message}";

                ModelState.AddModelError(string.Empty, errorString);
                logger.LogError(ex, $"{nameof(Action)}: Error getting child response for: {errorString}");
            }

            return View(MainRenderViewName, viewModel);
        }
    }
}