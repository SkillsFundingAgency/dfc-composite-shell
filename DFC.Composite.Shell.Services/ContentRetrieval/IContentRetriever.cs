using DFC.Composite.Shell.Models.AppRegistration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieval
{
    public interface IContentRetriever
    {
        Task<string> GetContentAsync(string url, string path, RegionModel regionModel, bool followRedirects, Uri requestBaseUrl);

        Task<string> PostContentAsync(string url, string path, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, Uri requestBaseUrl);
    }
}