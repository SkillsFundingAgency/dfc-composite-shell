using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AppRegistry
{
    public class AppRegistryRequestService : IAppRegistryRequestService
    {
        private readonly ILogger<AppRegistryRequestService> logger;
        private readonly HttpClient httpClient;

        public AppRegistryRequestService(ILogger<AppRegistryRequestService> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<AppRegistrationModel>> GetPaths()
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get, httpClient.BaseAddress);
            var response = await httpClient.SendAsync(msg);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<List<AppRegistrationModel>>();
        }

        public async Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            var patchUrl = new Uri($"{httpClient.BaseAddress}{path}/regions/{(int)pageRegion}", UriKind.Absolute);
            var regionPatchModel = new JsonPatchDocument<RegionModel>().Add(region => region.IsHealthy, isHealthy);
            var jsonRequest = JsonConvert.SerializeObject(regionPatchModel);
            using var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);

            try
            {
                var response = await httpClient.PatchAsync(patchUrl, content);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(
                    ex,
                    "{state}: BrokenCircuit: {patchUrl} - {message}, marking AppRegistration: {path}.{pageRegion} IsHealthy = {isHealthy}",
                    nameof(SetRegionHealthState),
                    patchUrl,
                    ex.Message,
                    path,
                    pageRegion,
                    isHealthy);

                return false;
            }
        }

        public async Task<bool> SetAjaxRequestHealthState(string path, string name, bool isHealthy)
        {
            var patchUrl = new Uri($"{httpClient.BaseAddress}{path}/ajaxrequests/{name}", UriKind.Absolute);
            var ajaxRequestPatchModel = new JsonPatchDocument<AjaxRequestModel>().Add(ajaxRequest => ajaxRequest.IsHealthy, isHealthy);
            var jsonRequest = JsonConvert.SerializeObject(ajaxRequestPatchModel);
            using var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);

            try
            {
                var response = await httpClient.PatchAsync(patchUrl, content);

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(
                    ex,
                    "{state}: BrokenCircuit: {patchUrl} - {message}, marking AppRegistration: {path}.{name} IsHealthy = {isHealthy}",
                    nameof(SetAjaxRequestHealthState),
                    patchUrl,
                    ex.Message,
                    path,
                    name,
                    isHealthy);

                return false;
            }
        }
    }
}