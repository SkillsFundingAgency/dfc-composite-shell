using DFC.Composite.Shell.Models.HealthModels;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
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

            logger.LogInformation($"Getting Health for: {model.Path}");

            var responseTask = await CallHttpClientJsonAsync(model);
            return responseTask;
        }

        private async Task<IEnumerable<HealthItemModel>> CallHttpClientJsonAsync(ApplicationHealthModel model)
        {
            logger.LogInformation($"{nameof(CallHttpClientJsonAsync)}: Loading health data for {model.Path} from {model.HealthUrl}");

            var request = new HttpRequestMessage(HttpMethod.Get, model.HealthUrl);

            if (!string.IsNullOrWhiteSpace(model.BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
            }

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            try
            {
                var response = await httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                        };
                        var result = JsonSerializer.Deserialize<List<HealthItemModel>>(responseString, options);

                        logger.LogInformation($"{nameof(CallHttpClientJsonAsync)}: Loaded health data for {model.Path} from {model.HealthUrl}");

                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"{nameof(CallHttpClientJsonAsync)}: Error loading health data for {model.Path} from {model.HealthUrl}: {ex.Message}");

                        var result = new List<HealthItemModel>
                        {
                            new HealthItemModel
                            {
                                Service = model.Path,
                                Message = $"Bad health response from {model.Path} app",
                                ResponseTime = 0,
                            },
                        };

                        return result;
                    }
                }
                else
                {
                    logger.LogError($"{nameof(CallHttpClientJsonAsync)}: Error loading health data for {model.Path} from {model.HealthUrl}: {response.StatusCode}");

                    var result = new List<HealthItemModel>
                    {
                        new HealthItemModel
                        {
                            Service = model.Path,
                            Message = $"No health response from {model.Path} app",
                            ResponseTime = 0,
                        },
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = new List<HealthItemModel>
                    {
                        new HealthItemModel
                        {
                            Service = model.Path,
                            Message = $"Exception response from {model.Path} app: {ex.Message}",
                            ResponseTime = 0,
                        },
                    };

                return result;
            }
        }
    }
}
