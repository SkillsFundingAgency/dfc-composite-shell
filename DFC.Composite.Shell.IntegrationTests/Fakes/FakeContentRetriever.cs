using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Services.ContentRetrieval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class FakeContentRetriever : IContentRetriever
    {
        public Task<string> GetContentAsync(
            string url,
            string path,
            RegionModel regionModel,
            bool followRedirects,
            Uri requestBaseUrl)
        {
            return Task.FromResult(Concat(
                "GET",
                url,
                path,
                regionModel?.PageRegion.ToString()));
        }

        public Task<string> PostContentAsync(
            string url,
            string path,
            RegionModel regionModel,
            IEnumerable<KeyValuePair<string, string>> formParameters,
            Uri requestBaseUrl)
        {
            return Task.FromResult(Concat(
                "POST",
                url,
                path,
                regionModel?.PageRegion.ToString(),
                string.Join(", ", formParameters.Select(kvp => string.Concat(kvp.Key, "=", kvp.Value)))));
        }

        private string Concat(params string[] values)
        {
            return string.Join(", ", values);
        }
    }
}
