using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.ContentRetrieval;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;
using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.IntegrationTests.Fakes
{
    public class FakeContentRetriever : IContentRetriever
    {
        public Task<string> GetContent(
            string url,
            string path,
            RegionModel regionModel,
            bool followRedirects,
            string requestBaseUrl,
            IHeaderDictionary headers)
        {
            return Task.FromResult(Concat(
                "GET",
                url,
                path,
                regionModel?.PageRegion.ToString()));
        }

        public Task<PostResponseModel> PostContent(
            string url,
            string path,
            RegionModel regionModel,
            IEnumerable<KeyValuePair<string, string>> formParameters,
            string requestBaseUrl)
        {
            return Task.FromResult(new PostResponseModel
            {
                HTML = Concat(
                    "POST",
                    url,
                    path,
                    regionModel?.PageRegion.ToString(),
                    string.Join(", ", formParameters.Select(kvp => string.Concat(kvp.Key, "=", kvp.Value)))),
            });
        }

        private string Concat(params string[] values)
        {
            return string.Join(", ", values);
        }
    }
}
