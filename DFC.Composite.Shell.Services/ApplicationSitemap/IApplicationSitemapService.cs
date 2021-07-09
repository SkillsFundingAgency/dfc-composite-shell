using DFC.Composite.Shell.Models.Sitemap;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationSitemap
{
    public interface IApplicationSitemapService
    {
        Task<ApplicationSitemapModel> EnrichAsync(ApplicationSitemapModel model);
    }
}