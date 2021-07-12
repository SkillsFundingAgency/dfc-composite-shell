using DFC.Composite.Shell.Services.Banner;

using Microsoft.AspNetCore.Html;

using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class TestBannerService : IBannerService
    {
        public Task<HtmlString> GetPageBannersAsync(string path) =>
            Task.FromResult(new HtmlString("some html"));
    }
}
