using DFC.Composite.Shell.Models.Sitemap;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
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
        private readonly ILogger<ApplicationSitemapService> logger;
        private readonly HttpClient httpClient;

        public ApplicationSitemapService(ILogger<ApplicationSitemapService> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "Existing swallow pattern")]
        public async Task<ApplicationSitemapModel> EnrichAsync(ApplicationSitemapModel model)
        {
            if (model == null)
            {
                return null;
            }

            try
            {
                logger.LogInformation("Getting Sitemap for: {path}", model.Path);

                var responseTask = await CallHttpClientXmlAsync<Sitemap>(model);
                model.Data = responseTask?.Locations;

                return model;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception getting Sitemap for: {path}", model.Path);
                return model;
            }
        }

        private async Task<T> CallHttpClientXmlAsync<T>(ApplicationSitemapModel model)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, model.SitemapUrl);
            if (!string.IsNullOrWhiteSpace(model.BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.BearerToken);
            }

            request.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Xml);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseString))
            {
                return default;
            }

            using var reader = new StringReader(responseString);
            using var xmlReader = XmlReader.Create(reader);

            xmlReader.Read();

            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlReader);
        }
    }
}