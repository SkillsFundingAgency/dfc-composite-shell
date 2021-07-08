using DFC.Composite.Shell.Integration.Test.Extensions;

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
            return WithWebHostBuilder(x =>
            {
                x.RegisterServices();
            }).CreateClient();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseApplicationInsights();

            builder?.ConfigureServices(services =>
            {
                var serviceProvider = services.BuildServiceProvider();
            });
        }
    }
}