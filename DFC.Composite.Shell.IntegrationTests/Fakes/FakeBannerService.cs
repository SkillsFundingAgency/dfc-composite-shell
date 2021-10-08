using DFC.Composite.Shell.Services.Banner;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.IntegrationTests.Fakes
{
    public class FakeBannerService : IBannerService
    {
        public Task<HtmlString> GetPageBannersAsync(string path) =>
            Task.FromResult(new HtmlString("some html"));
    }
}
