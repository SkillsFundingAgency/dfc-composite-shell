using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.ContentRetrieve;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class TestContentRetriever : IContentRetriever
    {
        public Task<string> GetContent(string url, RegionModel regionModel, bool followRedirects, string requestBaseUrl)
        {
            var seperator = ", ";
            return Task.FromResult(string.Concat(
                "GET", seperator,
                url, seperator,
                regionModel.Path, seperator,
                regionModel.PageRegion.ToString()));
        }

        public Task<string> PostContent(string url, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl)
        {
            var seperator = ", ";
            return Task.FromResult(string.Concat(
                "POST", seperator,
                url, seperator,
                regionModel.Path, seperator,
                regionModel.PageRegion.ToString()));
        }
    }
}
