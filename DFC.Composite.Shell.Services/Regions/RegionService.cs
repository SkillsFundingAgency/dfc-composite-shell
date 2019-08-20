using DFC.Composite.Shell.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Regions
{
    public class RegionService : IRegionService
    {
        private readonly HttpClient httpClient;

        public RegionService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<RegionModel>> GetRegions(string path)
        {
            var regionsUrl = $"{httpClient.BaseAddress}api/paths/{path}/regions";
            using (var msg = new HttpRequestMessage(HttpMethod.Get, regionsUrl))
            {
                var response = await httpClient.SendAsync(msg).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<IEnumerable<RegionModel>>().ConfigureAwait(false);
            }
        }

        public async Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            var regionsUrl = $"{httpClient.BaseAddress}api/paths/{path}/regions/{(int)pageRegion}";
            var regionPatchModel = new RegionPatchModel { IsHealthy = isHealthy };
            var jsonRequest = JsonConvert.SerializeObject(regionPatchModel);

            using (var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json))
            {
                var response = await httpClient.PatchAsync(regionsUrl, content).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
        }
    }
}