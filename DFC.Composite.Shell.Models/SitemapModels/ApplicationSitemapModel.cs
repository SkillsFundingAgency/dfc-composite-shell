using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Models.SitemapModels
{
    public class ApplicationSitemapModel
    {
        public string Path { get; set; }

        public string BearerToken { get; set; }

        public string SitemapUrl { get; set; }

        public Task<IEnumerable<SitemapLocation>> RetrievalTask { get; set; }
    }
}