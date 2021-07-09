using System.Net.Http;

namespace DFC.Composite.Shell.Services.UriSpecifcHttpClient
{
    public interface IUriSpecifcHttpClientFactory
    {
        public HttpClient GetClientForRegionEndpoint(string url);
    }
}
