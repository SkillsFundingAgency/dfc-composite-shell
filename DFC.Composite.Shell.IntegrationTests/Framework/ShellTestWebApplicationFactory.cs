using DFC.Composite.Shell.Integration.Test.Extensions;
using DFC.Composite.Shell.IntegrationTests.Fakes;
using DFC.Composite.Shell.Services.AppRegistry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace DFC.Composite.Shell.Integration.Test.Framework
{
    public class ShellTestWebApplicationFactory<TStartUp> : WebApplicationFactory<TStartUp>
        where TStartUp : class
    {
        public HttpClient CreateClientWithWebHostBuilder()
        {
            return WithWebHostBuilder(builder =>
            {
                builder.RegisterTestServices();
            }).CreateClient();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseApplicationInsights();

            builder?.ConfigureServices(services =>
            {
                services.AddTransient<IAppRegistryService, FakeAppRegistryService>();

                var serviceProvider = new ServiceCollection().BuildServiceProvider();
                services.BuildServiceProvider();
            });
        }
    }
}
