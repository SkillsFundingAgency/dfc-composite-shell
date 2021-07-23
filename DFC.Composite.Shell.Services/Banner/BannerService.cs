using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;

using Polly.CircuitBreaker;

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
            try
            {
#pragma warning disable CA2234 // Pass system uri objects instead of strings
                var response = await httpClient.GetAsync(path);
#pragma warning restore CA2234 // Pass system uri objects instead of strings

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Call to Banner app failed. Status: {response.StatusCode}. Message: {response.ReasonPhrase}");
                }

                logger.LogInformation($"Banners for path: {path} retrieved successfully.");

                return new HtmlString(await response.Content.ReadAsStringAsync());
            }
            catch (TaskCanceledException e)
            {
                logger.LogError(e, "Call to Banner app failed.");
            }
            catch (BrokenCircuitException e)
            {
                logger.LogError(e, "Call to Banner app failed.");
            }

            return new HtmlString(string.Empty);
        }
    }
}
