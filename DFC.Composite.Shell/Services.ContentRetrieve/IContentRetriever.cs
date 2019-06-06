using System.Threading.Tasks;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public interface IContentRetriever
    {
        Task<string> GetContent(string url, bool isHealthy, string offlineHtml);
    }
}
