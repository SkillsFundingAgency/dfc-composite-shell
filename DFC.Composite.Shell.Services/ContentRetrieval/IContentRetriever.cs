using DFC.Composite.Shell.Models.AppRegistrationModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieval
{
    public interface IContentRetriever
    {
        Task<string> GetContent(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl);

        Task<string> PostContent(string url, string path, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl);
    }
}