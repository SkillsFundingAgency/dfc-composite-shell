using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

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
            var regionsUrl = $"{_httpClient.BaseAddress}api/paths/{path}/regions/{(int)pageRegion}";
            var regionPatchModel = new RegionPatchModel()
            {
                IsHealthy = true
            };
            var jsonRequest = JsonConvert.SerializeObject(regionPatchModel);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await _httpClient.PatchAsync(regionsUrl, content);

            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAsUnhealthyAsync(string path, PageRegion pageRegion)
        {
            var regionsUrl = $"{_httpClient.BaseAddress}api/paths/{path}/regions/{(int)pageRegion}";
            var regionPatchModel = new RegionPatchModel()
            {
                IsHealthy = false
            };
            var jsonRequest = JsonConvert.SerializeObject(regionPatchModel);
            var content = new StringContent(jsonRequest, Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await _httpClient.PatchAsync(regionsUrl, content);

            response.EnsureSuccessStatusCode();
        }
    }
}
