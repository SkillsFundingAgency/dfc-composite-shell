using System;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public class ContentRetriever : IContentRetriever
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContentRetriever> _logger;

        public ContentRetriever(HttpClient httpClient, ILogger<ContentRetriever> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetContent(string url, bool isHealthy, string offlineHtml)
        {
            string results = null;

            try
            {
                if (isHealthy)
                {
                    var response = await _httpClient.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    results = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    if (!string.IsNullOrEmpty(offlineHtml))
                    {
                        results = offlineHtml;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Exception)}: {ex.Message}");

                if (!string.IsNullOrEmpty(offlineHtml))
                {
                    results = offlineHtml;
                }
            }

            return results;
        }
    }
}
