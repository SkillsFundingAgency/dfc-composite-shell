using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace DFC.Composite.Shell.Services.UriSpecifcHttpClient
{
    public class UriSpecifcHttpClientFactory : IUriSpecifcHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly List<string> registeredUrls;

        public UriSpecifcHttpClientFactory(IHttpClientFactory httpClientFactory, IRegisteredUrls registeredUrls)
        {
            this.httpClientFactory = httpClientFactory;
            this.registeredUrls = registeredUrls?.GetAll().Select(url => $"{url}_{nameof(UriSpecifcHttpClientFactory)}").ToList();
        }

        public HttpClient GetClientForRegionEndpoint(string url)
        {
            var key = $"{url}_{nameof(UriSpecifcHttpClientFactory)}";

            return string.IsNullOrEmpty(url) || !registeredUrls.Contains(key)
                ? httpClientFactory.CreateClient(RegisteredUrlConstants.DefaultKey)
                : httpClientFactory.CreateClient(key);
        }
    }
}