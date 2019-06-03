using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public class ContentRetriever : IContentRetriever
    {
        private readonly HttpClient _httpClient;

        public ContentRetriever(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetContent(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
