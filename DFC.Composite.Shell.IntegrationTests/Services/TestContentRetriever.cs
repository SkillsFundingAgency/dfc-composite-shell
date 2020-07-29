using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.ContentRetrieval;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class TestContentRetriever : IContentRetriever
    {
        private const string Seperator = ", ";

        public Task<string> GetContent(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl)
        {
            return Task.FromResult(Concat(
                "GET",
                url,
                path,
                regionModel?.PageRegion.ToString()));
        }

        public Task<string> PostContent(string url, string path, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl)
        {
            return Task.FromResult(Concat(
                "POST",
                url,
                path,
                regionModel?.PageRegion.ToString(),
                string.Join(", ", formParameters.Select(x => string.Concat(x.Key, "=", x.Value)))));
        }

        private string Concat(params string[] values)
        {
            return string.Join(Seperator, values);
        }
    }
}