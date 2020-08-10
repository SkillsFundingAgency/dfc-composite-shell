using DFC.Composite.Shell.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ViewComponents
{
    public class AppInsightsViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;

        public AppInsightsViewComponent(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Azure hosted app service environment variables takes precedence over config
            // see https://github.com/Microsoft/ApplicationInsights-aspnetcore/blob/v2.5.0/src/Microsoft.ApplicationInsights.AspNetCore/Extensions/ApplicationInsightsExtensions.cs#L286-L297
            var appInsightsKey = configuration.GetSection(Constants.AzureAppServiceAppInsightsInstrumentationKeyForWebSites)?.Value;
            appInsightsKey = string.IsNullOrEmpty(appInsightsKey) ? configuration.GetValue<string>(Constants.ApplicationInsightsInstrumentationKey) : appInsightsKey;

            var vm = new AppInsightsViewModel { InstrumentationKey = appInsightsKey };
            return await Task.FromResult(View(vm)).ConfigureAwait(true);
        }
    }
}