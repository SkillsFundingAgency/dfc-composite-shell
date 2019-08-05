using DFC.Composite.Shell.Models.SitemapModels;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DFC.Composite.Shell.Services.ApplicationSitemap
{
    public class ApplicationSitemapService : IApplicationSitemapService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<ApplicationSitemapService> logger;

        public ApplicationSitemapService(HttpClient httpClient, ILogger<ApplicationSitemapService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public string Path { get; set; }

        public string BearerToken { get; set; }

        public string SitemapUrl { get; set; }

        public Task<IEnumerable<SitemapLocation>> TheTask { get; set; }

        public async Task<IEnumerable<SitemapLocation>> GetAsync()
        {
            var data = await CallHttpClientXmlAsync<Sitemap>(SitemapUrl).ConfigureAwait(false);

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

                var response = await httpClient.SendAsync(request).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(responseString))
                {
                    using (var xmlReader = XmlReader.Create(reader))
                    {
                        xmlReader.Read();
                        return (T)serializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(Exception)}: {ex.Message}");
            }

            return default(T);
        }
    }
}