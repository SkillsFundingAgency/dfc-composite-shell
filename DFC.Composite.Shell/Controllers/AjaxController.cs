using DFC.Composite.Shell.Models.AjaxApi;
using DFC.Composite.Shell.Services.AjaxRequest;
using DFC.Composite.Shell.Services.AppRegistry;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AjaxController : ControllerBase
    {
        private readonly IAjaxRequestService ajaxRequestService;
        private readonly IAppRegistryService appRegistryDataService;

        public AjaxController(IAjaxRequestService ajaxRequestService, IAppRegistryService appRegistryDataService)
        {
            this.ajaxRequestService = ajaxRequestService;
            this.appRegistryDataService = appRegistryDataService;
        }

        [HttpGet]
        [Route("Action")]
        public async Task<IActionResult> Action([FromQuery] RequestModel requestModel)
        {
            if (string.IsNullOrWhiteSpace(requestModel?.Path) || string.IsNullOrWhiteSpace(requestModel?.Method))
            {
                return BadRequest();
            }

            var appRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(requestModel.Path);

            var ajaxRequest = appRegistrationModel?.AjaxRequests
                .FirstOrDefault(req => requestModel.Method?.Equals(req?.Name, StringComparison.OrdinalIgnoreCase) == true);

            if (string.IsNullOrWhiteSpace(ajaxRequest?.AjaxEndpoint))
            {
                return NotFound();
            }

            var result = await ajaxRequestService.GetResponseAsync(requestModel, ajaxRequest);

            return new ObjectResult(result)
            {
                StatusCode = (int)result.Status,
            };
        }
    }
}
