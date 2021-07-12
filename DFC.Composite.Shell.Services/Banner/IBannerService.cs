using Microsoft.AspNetCore.Html;

using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Banner
{
    public interface IBannerService
    {
        Task<HtmlString> GetPageBannersAsync(string path);
    }
}
