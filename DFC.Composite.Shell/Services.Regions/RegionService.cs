using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using Newtonsoft.Json;

namespace DFC.Composite.Shell.Services.Regions
{
    public class RegionService : IRegionService
    {
        private readonly HttpClient _httpClient;

        public RegionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var regionsUrl = $"{_httpClient.BaseAddress}api/paths/{path}/regions";
            var msg = new HttpRequestMessage(HttpMethod.Get, regionsUrl);

            var response = await _httpClient.SendAsync(msg);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<IEnumerable<RegionModel>>();
        }

        public async Task MarkAsHealthyAsync(string path, PageRegion pageRegion)
        {
            await MarkUnhealthStateAsync(path, pageRegion, true);
        }

        public async Task MarkAsUnhealthyAsync(string path, PageRegion pageRegion)
        {
            await MarkUnhealthStateAsync(path, pageRegion, false);
        }

        private async Task MarkUnhealthStateAsync(string path, PageRegion pageRegion, bool isHealthy)
        {
            var regionsUrl = $"{_httpClient.BaseAddress}api/paths/{path}/regions/{(int)pageRegion}";
            var regionPatchModel = new RegionPatchModel()
            {
                IsHealthy = isHealthy
            };
            var jsonRequest = JsonConvert.SerializeObject(regionPatchModel);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await _httpClient.PatchAsync(regionsUrl, content);

            response.EnsureSuccessStatusCode();
        }
    }
}
