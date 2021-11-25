using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DFC.Composite.Shell.Services.UriSpecificHttpClient
{
    public class UriSpecifcHttpClientFactory : IUriSpecifcHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly List<string> registeredUrlKeys;
        private readonly ILogger<UriSpecifcHttpClientFactory> logger;

        public UriSpecifcHttpClientFactory(
            IHttpClientFactory httpClientFactory,
            IRegisteredUrls registeredUrls,
            ILogger<UriSpecifcHttpClientFactory> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;

            registeredUrlKeys = registeredUrls?.GetAll().Select(url => $"{url.Url}_{nameof(UriSpecifcHttpClientFactory)}").ToList();
        }

        public HttpClient GetClientForRegionEndpoint(string url)
        {
            var key = $"{url}_{nameof(UriSpecifcHttpClientFactory)}";

            if (string.IsNullOrEmpty(url) || !registeredUrlKeys.Contains(key))
            {
                logger.LogInformation(
                    "Url key '{key}' was empty or not contained in registered url keys ({registeredUrlKeys}). Using default.",
                    key,
                    string.Join(',', registeredUrlKeys));

                key = $"{RegisteredUrlConstants.DefaultKey}_{nameof(UriSpecifcHttpClientFactory)}";
            }

            return httpClientFactory.CreateClient(key);
        }
    }
}