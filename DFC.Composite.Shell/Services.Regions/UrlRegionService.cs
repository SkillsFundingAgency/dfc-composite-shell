using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Regions
{
    public class UrlRegionService : IRegionService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public UrlRegionService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var pathUri = $"{_configuration["RegionApiUrl"]}paths/{path}/regions";
            var msg = new HttpRequestMessage(HttpMethod.Get, pathUri);

            var response = await _httpClient.SendAsync(msg);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<IEnumerable<RegionModel>>();
        }

        public Task MarkAsHealthy(string path, PageRegion region)
        {
            throw new NotImplementedException();
        }

        public Task MarkAsUnhealthy(string path, PageRegion region)
        {
            throw new NotImplementedException();
        }
    }
}
