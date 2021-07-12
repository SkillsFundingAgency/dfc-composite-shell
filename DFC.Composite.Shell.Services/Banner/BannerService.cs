using DFC.Composite.Shell.Models.Exceptions;

using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;

using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Banner
{
    public class BannerService : IBannerService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<BannerService> logger;

        public BannerService(HttpClient httpClient, ILogger<BannerService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<HtmlString> GetPageBannersAsync(string path)
        {
            logger.LogInformation($"Retrieving banners for path: {path}");

#pragma warning disable CA2234 // Pass system uri objects instead of strings
            var response = await httpClient.GetAsync(path);
#pragma warning restore CA2234 // Pass system uri objects instead of strings

            if (!response.IsSuccessStatusCode)
            {
                throw new EnhancedHttpException(response.StatusCode, response.ReasonPhrase, path);
            }

            logger.LogInformation($"Banners for path: {path} retrieved successfully.");

            return new HtmlString(await response.Content.ReadAsStringAsync());
        }
    }
}
