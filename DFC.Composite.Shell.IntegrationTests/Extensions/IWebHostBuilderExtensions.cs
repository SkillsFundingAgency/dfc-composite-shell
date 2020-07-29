using DFC.Composite.Shell.Integration.Test.Services;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.ContentRetrieval;
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
                services.AddTransient<IAppRegistryService, TestPathService>();
                services.AddTransient<IContentRetriever, TestContentRetriever>();
            });
        }
    }
}