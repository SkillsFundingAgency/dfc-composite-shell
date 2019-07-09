using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DFC.Composite.Shell.Models.Robots;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DFC.Composite.Shell.Services.ApplicationRobot
{
    public class ApplicationRobotService : IApplicationRobotService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApplicationRobotService> _logger;

        public ApplicationRobotService(HttpClient httpClient, ILogger<ApplicationRobotService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public string Path { get; set; }
        public string BearerToken { get; set; }
        public string RobotsURL { get; set; }
        public Task<string> TheTask { get; set; }

        public async Task<string> GetAsync()
        {
            var responseTask = await CallHttpClientTxtAsync(RobotsURL);

            return responseTask?.Data;
        }

        private async Task<Robot> CallHttpClientTxtAsync(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                }

                request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Text.Plain);

                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var result = new Robot();

                using (TextReader reader = new StringReader(responseString))
                {
                    result.Add(reader.ReadToEnd());

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Exception)}: {ex.Message}");
            }

            return null;
        }
    }
}
