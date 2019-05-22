using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public interface IContentRetriever
    {
        Task<string> GetContent(string url);
    }
}
