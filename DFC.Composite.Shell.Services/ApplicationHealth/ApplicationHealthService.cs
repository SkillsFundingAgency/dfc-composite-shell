using DFC.Composite.Shell.Models.HealthModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationHealth
{
    public class ApplicationHealthService : IApplicationHealthService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<ApplicationHealthService> logger;

        public ApplicationHealthService(HttpClient httpClient, ILogger<ApplicationHealthService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<IEnumerable<HealthItemModel>> GetAsync(ApplicationHealthModel model)
        {
            if (model == null)
            {
                return null;
            }

            var responseTask = await CallHttpClientJsonAsync(model).ConfigureAwait(false);

            return responseTask;
        }

        private async Task<IEnumerable<HealthItemModel>> CallHttpClientJsonAsync(ApplicationHealthModel model)
        {
            try
            {
                logger.LogInformation($"{nameof(CallHttpClientJsonAsync)}: Loading health data from {model.HealthUrl}");

                var request = new HttpRequestMessage(HttpMethod.Get, model.HealthUrl);

                if (!string.IsNullOrEmpty(model.BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
                }

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

                var response = await httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var result = JsonConvert.DeserializeObject<List<HealthItemModel>>(responseString);

                    logger.LogInformation($"{nameof(CallHttpClientJsonAsync)}: Loaded health data from {model.HealthUrl}");

                    return result;
                }
                else
                {
                    logger.LogError($"{nameof(CallHttpClientJsonAsync)}: Error loading health data from {model.HealthUrl}: {response.StatusCode}");

                    var result = new List<HealthItemModel>
                    {
                        new HealthItemModel
                        {
                            Service = model.Path,
                            Message = $"No health response from {model.HealthUrl} app",
                        },
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(CallHttpClientJsonAsync)}: {ex.Message}");

                var result = new List<HealthItemModel>
                {
                    new HealthItemModel
                    {
                        Service = model.Path,
                        Message = $"{ex.GetType().Name}: {ex.Message}",
                    },
                };

                return result;
            }
        }
    }
}
