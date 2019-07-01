using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public interface IContentRetriever
    {
        Task<string> GetContent(string url, bool isHealthy, string offlineHtml, bool followRedirects, string requestBaseUrl);
        Task<string> PostContent(string url, bool isHealthy, string offlineHtml, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl);
    }
}
