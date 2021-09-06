using DFC.Composite.Shell.IntegrationTests.Fakes;
using DFC.Composite.Shell.Services.Banner;
using DFC.Composite.Shell.Services.ContentRetrieval;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.Composite.Shell.Integration.Test.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder RegisterTestServices(this IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder.ConfigureTestServices(services =>
            {
                services.AddTransient<IContentRetriever, FakeContentRetriever>();
                services.AddTransient<IBannerService, FakeBannerService>();
            });
        }
    }
}
