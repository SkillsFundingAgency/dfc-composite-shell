using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Services.Paths
{
    public class PathService : IPathService
    {
        private readonly HttpClient _httpClient;

        public PathService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var pathsUrl = $"{_httpClient.BaseAddress}api/paths";
            var msg = new HttpRequestMessage(HttpMethod.Get, pathsUrl);

            var response = await _httpClient.SendAsync(msg);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<List<PathModel>>();
        }

    }
}
