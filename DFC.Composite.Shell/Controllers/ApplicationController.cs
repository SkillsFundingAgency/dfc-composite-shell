using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Exceptions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell.Controllers
{
    public class ApplicationController : BaseController
    {
        private const string MainRenderViewName = "Application/RenderView";

        private readonly IMapper<ApplicationModel, PageViewModel> _mapper;
        private readonly ILogger<ApplicationController> _logger;
        private readonly IApplicationService _applicationService;

        public ApplicationController(IMapper<ApplicationModel, PageViewModel> mapper,
            ILogger<ApplicationController> logger, 
            IConfiguration configuration, 
            IApplicationService applicationService,
            IVersionedFiles versionedFiles)
        : base(configuration, versionedFiles)
        {
            _mapper = mapper;
            _logger = logger;
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(ActionGetRequestModel requestViewModel)
        {
            var viewModel = BuildDefaultPageViewModel();

            try
            {
                _logger.LogInformation($"{nameof(Action)}: Getting child response for: {requestViewModel.Path}");

                var application = await _applicationService.GetApplicationAsync(requestViewModel.Path);

                if (application == null || application.Path == null)
                {
                    string errorString = string.Format(Messages.PathNotRegistered, requestViewModel.Path);

                    ModelState.AddModelError(string.Empty, errorString);
                    _logger.LogWarning($"{nameof(Action)}: {errorString}");
                }
                else
                {
                    _mapper.Map(application, viewModel);

                    _applicationService.RequestBaseUrl = BaseUrl();

                    await _applicationService.GetMarkupAsync(application, requestViewModel.Data + Request.QueryString, viewModel);

                    _logger.LogInformation($"{nameof(Action)}: Received child response for: {requestViewModel.Path}");
                }
            }
            catch(RedirectException ex)
            {
                _logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation.PathAndQuery} to: {ex.Location.PathAndQuery}");

                Response.Redirect(ex.Location.PathAndQuery, true);
            }
            catch (Exception ex)
            {
                var errorString = $"{requestViewModel.Path}: {ex.Message}";

                ModelState.AddModelError(string.Empty, errorString);
                _logger.LogError(ex, $"{nameof(Action)}: Error getting child response for: {errorString}");
            }

            return View(MainRenderViewName, viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Action(ActionPostRequestModel requestViewModel)
        {
            var viewModel = BuildDefaultPageViewModel();

            try
            {
                _logger.LogInformation($"{nameof(Action)}: Posting child request for: {requestViewModel.Path}");

                var application = await _applicationService.GetApplicationAsync(requestViewModel.Path);

                if (application == null || application.Path == null)
                {
                    string errorString = string.Format(Messages.PathNotRegistered, requestViewModel.Path);

                    ModelState.AddModelError(string.Empty, errorString);
                    _logger.LogWarning($"{nameof(Action)}: {errorString}");
                }
                else
                {
                    _mapper.Map(application, viewModel);

                    _applicationService.RequestBaseUrl = BaseUrl();

                    var formParameters = (from a in requestViewModel.FormCollection select new KeyValuePair<string, string>(a.Key, a.Value)).ToArray();

                    await _applicationService.PostMarkupAsync(application, requestViewModel.Path, requestViewModel.Data, formParameters, viewModel);

                    _logger.LogInformation($"{nameof(Action)}: Received child response for: {requestViewModel.Path}");
                }
            }
            catch (RedirectException ex)
            {
                _logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation.PathAndQuery} to: {ex.Location.PathAndQuery}");

                Response.Redirect(ex.Location.PathAndQuery, true);
            }
            catch (Exception ex)
            {
                var errorString = $"{requestViewModel.Path}: {ex.Message}";

                ModelState.AddModelError(string.Empty, errorString);
                _logger.LogError(ex, $"{nameof(Action)}: Error getting child response for: {errorString}");
            }

            return View(MainRenderViewName, viewModel);
        }

    }
}