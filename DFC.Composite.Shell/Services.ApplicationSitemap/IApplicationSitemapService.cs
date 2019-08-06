using DFC.Composite.Shell.Models.SitemapModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationSitemap
{
    public interface IApplicationSitemapService
    {
        Task<IEnumerable<SitemapLocation>> GetAsync(ApplicationSitemapModel model);
    }
}