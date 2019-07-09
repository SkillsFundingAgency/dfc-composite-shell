using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DFC.Composite.Shell.Models.Sitemap;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DFC.Composite.Shell.Services.ApplicationSitemap
{
    public class ApplicationSitemapService : IApplicationSitemapService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApplicationSitemapService> _logger;

        public ApplicationSitemapService(HttpClient httpClient, ILogger<ApplicationSitemapService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public string Path { get; set; }
        public string BearerToken { get; set; }
        public string SitemapUrl { get; set; }
        public Task<IEnumerable<SitemapLocation>> TheTask { get; set; }

        public async Task<IEnumerable<SitemapLocation>> GetAsync()
        {
            var data = await CallHttpClientXmlAsync<Sitemap>(SitemapUrl);

            return data?.Locations;
        }

        private async Task<T> CallHttpClientXmlAsync<T>(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                }

                request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Xml);

                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var serializer = new XmlSerializer(typeof(T));

                using (TextReader reader = new StringReader(responseString))
                {
                    var result = (T)serializer.Deserialize(reader);

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Exception)}: {ex.Message}");
            }

            return default(T);
        }
    }
}
