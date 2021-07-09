using DFC.Composite.Shell.Models.Health;
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

        public async Task<ApplicationHealthModel> EnrichAsync(ApplicationHealthModel model)
        {
            if (model == null)
            {
                return null;
            }

            model.Data = await CallHttpClientJsonAsync(model);
            return model;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "Continuing to use existing swallow pattern")]
        private async Task<IEnumerable<HealthItemModel>> CallHttpClientJsonAsync(ApplicationHealthModel model)
        {
            logger.LogInformation(
                "{name}: Loading health data from {healthUrl}",
                nameof(CallHttpClientJsonAsync),
                model.HealthUrl);

            var request = new HttpRequestMessage(HttpMethod.Get, model.HealthUrl);

            if (!string.IsNullOrWhiteSpace(model.BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
            }

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

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

                        logger.LogInformation(
                            "{name}: Loaded health data from {healthUrl}",
                            nameof(CallHttpClientJsonAsync),
                            model.HealthUrl);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            "{name}: Error loading health data from {healthUrl}: {message}",
                            nameof(CallHttpClientJsonAsync),
                            model.HealthUrl,
                            ex.Message);

                        return new List<HealthItemModel>
                        {
                            new HealthItemModel
                            {
                                Service = model.Path,
                                Message = $"Bad health response from {model.HealthUrl} app",
                            },
                        };
                    }
                }
                else
                {
                    logger.LogError(
                        "{name}: Error loading health data from {url}: {statusCode}",
                        nameof(CallHttpClientJsonAsync),
                        model.HealthUrl,
                        response.StatusCode);

                    return new List<HealthItemModel>
                    {
                        new HealthItemModel
                        {
                            Service = model.Path,
                            Message = $"No health response from {model.HealthUrl} app",
                        },
                    };
                }
            }
            catch (Exception ex)
            {
                return new List<HealthItemModel>
                {
                    new HealthItemModel
                    {
                        Service = model.Path,
                        Message = $"Exception response from {model.HealthUrl} app: {ex.Message}",
                    },
                };
            }
        }
    }
}
