using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.SimpeCachedObject;

namespace DFC.Composite.Shell.Services.Paths
{
    public class PathService : IPathService
    {
        private readonly HttpClient _httpClient;
        private readonly ISimpeCachedObjectService<List<PathModel>> _simpleCachedPathList;

        public PathService(HttpClient httpClient, ISimpeCachedObjectService<List<PathModel>> simpleCachedPathList)
        {
            _httpClient = httpClient;
            _simpleCachedPathList = simpleCachedPathList;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var pathModels = _simpleCachedPathList.CachedObject;

            if (pathModels == null)
            {
                var pathsUrl = $"{_httpClient.BaseAddress}api/paths";
                var msg = new HttpRequestMessage(HttpMethod.Get, pathsUrl);

                var response = await _httpClient.SendAsync(msg);

                response.EnsureSuccessStatusCode();

                pathModels = await response.Content.ReadAsAsync<List<PathModel>>();

                _simpleCachedPathList.CachedObject = pathModels;
            }

            return pathModels;
        }

        public async Task<PathModel> GetPath(string path)
        {
            var pathModels = await GetPaths();

            return pathModels.FirstOrDefault(s => s.Path == path.ToLower());
        }

    }
}
