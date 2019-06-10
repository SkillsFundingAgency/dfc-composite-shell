using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models.Sitemap;

namespace DFC.Composite.Shell.Services.ApplicationSitemap
{
    public interface IApplicationSitemapService
    {
        string Path { get; set; }
        string BearerToken { get; set; }
        string SitemapUrl { get; set; }
        Task<IEnumerable<SitemapLocation>> TheTask { get; set; }

        Task<IEnumerable<SitemapLocation>> GetAsync();
    }
}
