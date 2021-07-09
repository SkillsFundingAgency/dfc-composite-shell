using DFC.Composite.Shell.Models.AjaxApi;
using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Services.AppRegistry;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Polly.CircuitBreaker;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AjaxRequest
{
    public class AjaxRequestService : IAjaxRequestService
    {
        private readonly ILogger<AjaxRequestService> logger;
        private readonly IAppRegistryService appRegistryDataService;
        private readonly HttpClient httpClient;

        public AjaxRequestService(
            ILogger<AjaxRequestService> logger,
            IAppRegistryService appRegistryDataService,
            HttpClient httpClient)
        {
            this.logger = logger;
            this.appRegistryDataService = appRegistryDataService;
            this.httpClient = httpClient;
        }

        public async Task<ResponseModel> GetResponseAsync(RequestModel requestModel, AjaxRequestModel ajaxRequest)
        {
            _ = requestModel ?? throw new ArgumentNullException(nameof(requestModel));
            _ = ajaxRequest ?? throw new ArgumentNullException(nameof(ajaxRequest));

            var appData = string.IsNullOrWhiteSpace(requestModel.AppData) ?
                string.Empty : $"/{Uri.EscapeDataString(requestModel.AppData)}";

            var url = ajaxRequest.AjaxEndpoint.Replace("/{0}", $"{appData}", StringComparison.OrdinalIgnoreCase);

            var responseModel = new ResponseModel
            {
                Status = HttpStatusCode.OK,
                StatusMessage = "Unhealthy",
                IsHealthy = ajaxRequest.IsHealthy,
                OfflineHtml = ajaxRequest.OfflineHtml,
            };

            logger.LogInformation("Ajax request using: {url}", url);

            if (!ajaxRequest.IsHealthy)
            {
                return responseModel;
            }

            try
            {
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

                var response = await httpClient.GetAsync(new Uri(url, UriKind.Absolute));

                responseModel.Status = response.StatusCode;
                responseModel.StatusMessage = response.ReasonPhrase;

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Ajax request successful: {url}", url);
                    responseModel.Payload = await response.Content.ReadAsStringAsync();
                    responseModel.OfflineHtml = null;
                }
                else
                {
                    logger.LogError(
                        "Ajax request error: {status}: {statusMessage}, using: {url}",
                        responseModel.Status,
                        responseModel.StatusMessage,
                        url);
                }
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, "BrokenCircuit: {url} - {message}", url, ex.Message);

                responseModel.IsHealthy = false;

                if (ajaxRequest.HealthCheckRequired)
                {
                    await appRegistryDataService.SetAjaxRequestHealthState(requestModel.Path, ajaxRequest.Name, false);
                }
            }

            return responseModel;
        }
    }
}
