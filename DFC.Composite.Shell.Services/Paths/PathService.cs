using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Paths
{
    public class PathService : IPathService
    {
        private readonly HttpClient httpClient;

        public PathService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var pathsUrl = $"{httpClient.BaseAddress}api/paths";
            using (var msg = new HttpRequestMessage(HttpMethod.Get, pathsUrl))
            {
                var response = await httpClient.SendAsync(msg).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

                return await JsonSerializer.DeserializeAsync<List<PathModel>>(responseContent, options);
                //return await response.Content.ReadAsAsync<List<PathModel>>().ConfigureAwait(false);
            }
        }
    }
}