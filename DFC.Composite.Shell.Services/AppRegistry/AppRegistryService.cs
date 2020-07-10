using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
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
    public class AppRegistryService : IAppRegistryService
    {
        private readonly ILogger<AppRegistryService> logger;
        private readonly HttpClient httpClient;

        public AppRegistryService(ILogger<AppRegistryService> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<AppRegistrationModel>> GetPaths()
        {
            using (var msg = new HttpRequestMessage(HttpMethod.Get, httpClient.BaseAddress))
            {
                var response = await httpClient.SendAsync(msg).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<List<AppRegistrationModel>>().ConfigureAwait(false);
            }
        }

        public async Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            var patchUrl =new Uri( $"{httpClient.BaseAddress}{path}/regions/{(int)pageRegion}", UriKind.Absolute);
            var regionPatchModel = new JsonPatchDocument<RegionModel>().Add(x => x.IsHealthy, isHealthy);
            var jsonRequest = JsonConvert.SerializeObject(regionPatchModel);
            using var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);

            try
            {
                var response = await httpClient.PatchAsync(patchUrl, content).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(SetRegionHealthState)}: BrokenCircuit: {patchUrl} - {ex.Message}");

                return false;
            }
        }
    }
}