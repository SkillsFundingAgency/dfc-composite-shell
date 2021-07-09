using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using DFC.Composite.Shell.Models.Health;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.TokenRetriever;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class HealthController : Controller
    {
        private readonly IAppRegistryService appRegistryDataService;
        private readonly ILogger<HealthController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IApplicationHealthService applicationHealthService;

        public HealthController(
            IAppRegistryService appRegistryDataService,
            ILogger<HealthController> logger,
            IBearerTokenRetriever bearerTokenRetriever,
            IApplicationHealthService applicationHealthService)
        {
            this.appRegistryDataService = appRegistryDataService;
            this.logger = logger;
            this.bearerTokenRetriever = bearerTokenRetriever;
            this.applicationHealthService = applicationHealthService;
        }

        [HttpGet]
        [Route("health")]
        public async Task<IActionResult> Health()
        {
            var resourceName = typeof(Program).Namespace;

            logger.LogInformation("{health} has been called", nameof(Health));

            var message = "Composite Shell is available";
            logger.LogInformation("{health} responded with: {resourceName} - {message}", nameof(Health), resourceName, message);

            var viewModel = CreateHealthViewModel(resourceName, message);

            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            var onlineAppRegistrationModels = appRegistrationModels.Where(model => model.IsOnline && model.ExternalURL == null).ToList();

            if (onlineAppRegistrationModels?.Any() == true)
            {
                var applicationOnlineHealthModels = await GetApplicationOnlineHealthAsync(onlineAppRegistrationModels);
                AppendApplicationsHealths(viewModel.HealthItems, applicationOnlineHealthModels);
            }

            var offlineAppRegistrationModels = appRegistrationModels.Where(model => !model.IsOnline && model.ExternalURL == null).ToList();

            if (offlineAppRegistrationModels?.Any() == true)
            {
                var applicationOfflineHealthItemModels = GetApplicationOfflineHealth(offlineAppRegistrationModels);
                viewModel.HealthItems.AddRange(applicationOfflineHealthItemModels);
            }

            return this.NegotiateContentResult(viewModel);
        }

        [HttpGet]
        [Route("health/ping")]
        public IActionResult Ping()
        {
            logger.LogInformation("{ping} has been called", nameof(Ping));
            return Ok();
        }

        private static HealthViewModel CreateHealthViewModel(string resourceName, string message)
        {
            return new HealthViewModel
            {
                HealthItems = new List<HealthItemViewModel>
                {
                    new HealthItemViewModel
                    {
                        Service = resourceName,
                        Message = message,
                    },
                },
            };
        }

        private async Task<List<ApplicationHealthModel>> GetApplicationOnlineHealthAsync(IList<AppRegistrationModel> paths)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext) : null;
            var modelsTasks = paths.Select(path => GetModelForPath(path, bearerToken)).ToList();

            var applicationHealthModels = new List<ApplicationHealthModel>();

            foreach (var modelsTask in modelsTasks)
            {
                applicationHealthModels.Add(await modelsTask);
            }

            return applicationHealthModels;
        }

        private async Task<ApplicationHealthModel> GetModelForPath(AppRegistrationModel path, string bearerToken)
        {
            logger.LogInformation("{action}: Getting child Health for: {path}", nameof(Action), path.Path);
            var applicationBaseUrl = await GetPathBaseUrlFromBodyRegionAsync(path.Path);

            var applicationHealthModel = new ApplicationHealthModel
            {
                Path = path.Path,
                BearerToken = bearerToken,
                HealthUrl = $"{applicationBaseUrl}/health",
            };

            return await applicationHealthService.EnrichAsync(applicationHealthModel);
        }

        private async Task<string> GetPathBaseUrlFromBodyRegionAsync(string path)
        {
            var appRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(path);
            var bodyRegion = appRegistrationModel?.Regions?.FirstOrDefault(region => region.PageRegion == PageRegion.Body);

            if (!string.IsNullOrWhiteSpace(bodyRegion?.RegionEndpoint))
            {
                var uri = new Uri(bodyRegion.RegionEndpoint);
                var url = $"{uri.Scheme}://{uri.Authority}";

                return url;
            }

            return null;
        }

        private void AppendApplicationsHealths(
            List<HealthItemViewModel> healthItemModels,
            IEnumerable<ApplicationHealthModel> applicationHealthModels)
        {
            foreach (var applicationHealthModel in applicationHealthModels)
            {
                logger.LogInformation("{action}: Received child Health for: {path}", nameof(Action), applicationHealthModel.Path);
                var healthItems = applicationHealthModel.Data;

                if (healthItems?.Any() != true)
                {
                    var healthItemViewModel = new HealthItemViewModel
                    {
                        Service = applicationHealthModel.Path,
                        Message = $"No health response from {applicationHealthModel.Path} app",
                    };

                    healthItemModels.Add(healthItemViewModel);
                    continue;
                }

                var healthItemViewModels = healthItems
                    .Select(healthItem => new HealthItemViewModel { Service = healthItem.Service, Message = healthItem.Message })
                    .ToList();

                healthItemModels.AddRange(healthItemViewModels);
            }
        }

        private List<HealthItemViewModel> GetApplicationOfflineHealth(IList<AppRegistrationModel> paths)
        {
            var healthItemViewModels = new List<HealthItemViewModel>();

            foreach (var path in paths)
            {
                logger.LogInformation("{action}: Skipping health check for: {path}, because it is offline", nameof(Action), path.Path);

                var healthItemViewModel = new HealthItemViewModel
                {
                    Service = path.Path,
                    Message = $"Skipped health check for: {path.Path}, because it is offline",
                };

                healthItemViewModels.Add(healthItemViewModel);
            }

            return healthItemViewModels;
        }
    }
}
