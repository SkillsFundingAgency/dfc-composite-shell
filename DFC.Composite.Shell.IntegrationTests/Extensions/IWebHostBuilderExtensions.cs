using DFC.Composite.Shell.Integration.Test.Services;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DFC.Composite.Shell.Integration.Test.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder RegisterServices(this IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder.ConfigureTestServices(services =>
            {
                services.AddTransient<IPathService, TestPathService>();
                services.AddTransient<IRegionService, TestRegionService>();
                services.AddTransient<IContentRetriever, TestContentRetriever>();
            });
        }
    }
}