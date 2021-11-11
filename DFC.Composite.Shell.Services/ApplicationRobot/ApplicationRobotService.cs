using DFC.Composite.Shell.Models.Robots;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationRobot
{
    public class ApplicationRobotService : IApplicationRobotService
    {
        private readonly ILogger<ApplicationRobotService> logger;
        private readonly HttpClient httpClient;

        public ApplicationRobotService(ILogger<ApplicationRobotService> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<string> GetAsync(ApplicationRobotModel model)
        {
            if (model == null)
            {
                return null;
            }

            try
            {
                logger.LogInformation($"Getting Robots.txt for: {model.Path}");

                var responseTask = await CallHttpClientTxtAsync(model);
                return responseTask?.Data;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception getting Robots.txt for: {model.Path}");

                return null;
            }
        }

        private async Task<Robot> CallHttpClientTxtAsync(ApplicationRobotModel model)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, model.RobotsURL);
            if (!string.IsNullOrWhiteSpace(model.BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
            }

            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Text.Plain);

            var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var result = new Robot();

            using var reader = new StringReader(responseString);
            result.Append(await reader.ReadToEndAsync());
            return result;
        }
    }
}