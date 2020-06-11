using DFC.Composite.Shell.Models.SitemapModels;
using Microsoft.Net.Http.Headers;
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

        public ApplicationSitemapService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<SitemapLocation>> GetAsync(ApplicationSitemapModel model)
        {
            if (model == null)
            {
                return null;
            }

            var responseTask = await CallHttpClientXmlAsync<Sitemap>(model).ConfigureAwait(false);
            return responseTask?.Locations;
        }

        private async Task<T> CallHttpClientXmlAsync<T>(ApplicationSitemapModel model)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, model.SitemapUrl))
            {
                if (!string.IsNullOrWhiteSpace(model.BearerToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
                }

                request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Xml);

                var response = await httpClient.SendAsync(request).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(responseString))
                {
                    return default;
                }

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
        }
    }
}