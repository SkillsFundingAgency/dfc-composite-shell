using System.Collections.Concurrent;
using System.Net.Http;

namespace DFC.Composite.Shell.Services.UriSpecifcHttpClient
{
    public class UriSpecifcHttpClientFactory : IUriSpecifcHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ConcurrentDictionary<string, bool> registedUrls = new ConcurrentDictionary<string, bool>();

        public UriSpecifcHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public HttpClient GetClientForRegionEndpoint(string url)
        {
            return string.IsNullOrEmpty(url) || !registedUrls.ContainsKey(url)
                ? httpClientFactory.CreateClient(RegisteredUrlConstants.DefaultKey)
                : httpClientFactory.CreateClient(url);
        }

        public void RegisterUrl(string url)
        {
            registedUrls.TryAdd(url, true);
        }
    }
}