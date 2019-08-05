using DFC.Composite.Shell.Models.SitemapModels;
using System.Collections.Generic;
using System.Threading.Tasks;

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