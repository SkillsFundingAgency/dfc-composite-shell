using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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
            var pathsUrl = $"{_httpClient.BaseAddress}/api/paths";
            var msg = new HttpRequestMessage(HttpMethod.Get, pathsUrl);

            var response = await _httpClient.SendAsync(msg);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<List<PathModel>>();
        }

        public async Task<PathModel> GetPath(string path)
        {
            var pathUrl = $"{_httpClient.BaseAddress}/api/paths/{path}";
            var msg = new HttpRequestMessage(HttpMethod.Get, pathUrl);

            var response = await _httpClient.SendAsync(msg);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<PathModel>();
        }

    }
}
