using System;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public class MockContentRetriever : IContentRetriever
    {
        public async Task<string> GetContent(string url)
        {
            await Task.CompletedTask;
            return $"Hello. {DateTime.Now} {url}<br/>";
        }
    }
}
