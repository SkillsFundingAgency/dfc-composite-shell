using DFC.Composite.Shell.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Paths
{
    public class UrlPathService : IPathService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public UrlPathService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var pathUri = $"{_configuration["PathApiUrl"]}paths";
            var msg = new HttpRequestMessage(HttpMethod.Get, pathUri);

            var response = await _httpClient.SendAsync(msg);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<List<PathModel>>();
        }

    }
}
