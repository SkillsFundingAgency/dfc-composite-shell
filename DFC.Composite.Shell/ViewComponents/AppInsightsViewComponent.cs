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

        public Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new AppInsightsViewModel
            {
                InstrumentationKey = GetAppInsightsKeyPreferringAppServiceKey(),
            };

            return Task.FromResult((IViewComponentResult)View(viewModel));
        }

        private string GetAppInsightsKeyPreferringAppServiceKey()
        {
            // Azure hosted app service environment variables takes precedence over config
            // see https://github.com/Microsoft/ApplicationInsights-aspnetcore/blob/v2.5.0/src/Microsoft.ApplicationInsights.AspNetCore/Extensions/ApplicationInsightsExtensions.cs#L286-L297
            var appServiceAppInsightsKey = configuration.GetSection(Constants.AzureAppServiceAppInsightsInstrumentationKeyForWebSites)?.Value;

            return string.IsNullOrEmpty(appServiceAppInsightsKey) ?
                configuration.GetValue<string>(Constants.ApplicationInsightsInstrumentationKey)
                : appServiceAppInsightsKey;
        }
    }
}
