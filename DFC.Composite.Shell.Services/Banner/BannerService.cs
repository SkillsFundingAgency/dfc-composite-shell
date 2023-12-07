using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Polly.CircuitBreaker;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Banner
{
    public class BannerService : IBannerService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<BannerService> logger;
        private readonly IMemoryCache memoryCache;

        public BannerService(HttpClient httpClient, ILogger<BannerService> logger, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        public async Task<HtmlString> GetPageBannersAsync(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            const int CacheDurationInSeconds = 600;
            var cacheKey = BuildCacheKey(path);

            if (!memoryCache.TryGetValue(cacheKey, out HtmlString content))
            {
                content = await GetPageBanners_WithoutCachingAsync(path);
                memoryCache.Set(cacheKey, content, TimeSpan.FromSeconds(CacheDurationInSeconds));
            }

            return content;
        }

        private string BuildCacheKey(string path)
        {
            return $"{nameof(BannerService)}_{path}";
        }

        private async Task<HtmlString> GetPageBanners_WithoutCachingAsync(string path)
        {
            logger.LogInformation($"Retrieving banners for path: {path}");
            try
            {
#pragma warning disable CA2234 // Pass system uri objects instead of strings
                var response = await httpClient.GetAsync(path.TrimStart('/'));
#pragma warning restore CA2234 // Pass system uri objects instead of strings

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation($"Banners for path: {path} retrieved successfully.");

                    return new HtmlString(await response.Content.ReadAsStringAsync());
                }

                logger.LogError($"Call to Banner app failed. Status: {response.StatusCode}. Message: {response.ReasonPhrase}");
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
