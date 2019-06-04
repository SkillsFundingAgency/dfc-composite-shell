using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DFC.Composite.Shell.Models.Sitemap;

namespace DFC.Composite.Shell.Services.ApplicationSitemap
{
    public class ApplicationSitemapService : IApplicationSitemapService
    {
        private readonly HttpClient _httpClient;

        public ApplicationSitemapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string BearerToken { get; set; }
        public string SitemapUrl { get; set; }
        public Task<IEnumerable<SitemapLocation>> TheTask { get; set; }

        public async Task<IEnumerable<SitemapLocation>> GetAsync()
        {
            var data = await CallHttpClientXmlAsync<Sitemap>(SitemapUrl);

            return data.Locations;
        }

        private async Task<T> CallHttpClientXmlAsync<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
            }

            request.Headers.Add("Accept", "application/xml");

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
    }
}
