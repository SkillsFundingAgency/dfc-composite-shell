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
            var appInsightsKey = configuration.GetValue<string>(Constants.ApplicationInsightsInstrumentationKey);

            var vm = new AppInsightsViewModel { InstrumentationKey = appInsightsKey };
            return await Task.FromResult(View(vm)).ConfigureAwait(true);
        }
    }
}