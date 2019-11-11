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
        private readonly HttpClient httpClient;
        private readonly ILogger<ApplicationRobotService> logger;

        public ApplicationRobotService(HttpClient httpClient, ILogger<ApplicationRobotService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<string> GetAsync(ApplicationRobotModel model)
        {
            if (model == null)
            {
                return null;
            }

            var responseTask = await CallHttpClientTxtAsync(model).ConfigureAwait(false);
            return responseTask?.Data;
        }

        private async Task<Robot> CallHttpClientTxtAsync(ApplicationRobotModel model)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, model.RobotsURL);

                if (!string.IsNullOrEmpty(model.BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
                }

                request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Text.Plain);

                var response = await httpClient.SendAsync(request).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = new Robot();

                using (var reader = new StringReader(responseString))
                {
                    result.Append(reader.ReadToEnd());
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(Exception)}: {ex.Message}");
            }

            return null;
        }
    }
}