using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public class RealContentRetriever : IContentRetriever
    {
        private readonly HttpClient _httpClient;

        public RealContentRetriever(HttpClient httpClient)
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
