using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class ApplicationController : BaseController
    {
        private const string MainRenderViewName = "Application/RenderView";

        private readonly IMapper<ApplicationModel, PageViewModel> _mapper;
        private readonly ILogger<ApplicationController> _logger;
        private readonly IApplicationService _applicationService;

        public ApplicationController(IMapper<ApplicationModel, PageViewModel> mapper, ILogger<ApplicationController> logger, IConfiguration configuration, IApplicationService applicationService)
        :base(configuration)
        {
            _mapper = mapper;
            _logger = logger;
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(ActionGetRequestModel requestViewModel)
        {
            var vm = new PageViewModel
            {
                BrandingAssetsCdn = _configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn))
            };

            try
            {
                var application = await _applicationService.GetApplicationAsync(requestViewModel.Path);

                if (application == null)
                {
                    ModelState.AddModelError(string.Empty, string.Format(Messages.PathNotRegistered, requestViewModel.Path));
                }
                else
                {
                    _mapper.Map(application, vm);

                    await _applicationService.GetMarkupAsync(requestViewModel.Path, requestViewModel.Data, vm);
                }
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(BrokenCircuitException)} {ex.Message}");
                var errorString = $"{requestViewModel.Path}: BrokenCircuit: {ex.Message}";
                ModelState.AddModelError(string.Empty, errorString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Exception)} {ex.Message}");
                var errorString = $"{requestViewModel.Path}: {ex.Message}";
                ModelState.AddModelError(string.Empty, errorString);
            }

            return View(MainRenderViewName, vm);
        }

    }
}