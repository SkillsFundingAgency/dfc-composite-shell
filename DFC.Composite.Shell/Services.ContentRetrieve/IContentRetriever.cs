using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public interface IContentRetriever
    {
        Task<string> GetContent(string url, RegionModel regionModel, bool followRedirects, string requestBaseUrl);
        Task<string> PostContent(string url, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl);
    }
}
