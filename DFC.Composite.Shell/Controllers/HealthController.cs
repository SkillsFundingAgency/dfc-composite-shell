using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.HealthModels;
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
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly ILogger<HealthController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IApplicationHealthService applicationHealthService;

        public HealthController(
            IAppRegistryDataService appRegistryDataService,
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
            string resourceName = typeof(Program).Namespace;
            string message;

            logger.LogInformation($"{nameof(Health)} has been called");

            message = "Composite Shell is available";
            logger.LogInformation($"{nameof(Health)} responded with: {resourceName} - {message}");

            var viewModel = CreateHealthViewModel(resourceName, message);

            // loop through the registered applications and create some tasks - one per application for their health
            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            var onlineAppRegistrationModels = appRegistrationModels.Where(w => w.IsOnline && w.ExternalURL == null).ToList();
            var offlineAppRegistrationModels = appRegistrationModels.Where(w => !w.IsOnline && w.ExternalURL == null).ToList();

            if (onlineAppRegistrationModels != null && onlineAppRegistrationModels.Any())
            {
                var applicationOnlineHealthModels = await GetApplicationOnlineHealthAsync(onlineAppRegistrationModels);

                AppendApplicationsHealths(viewModel.HealthItems, applicationOnlineHealthModels);
            }

            if (offlineAppRegistrationModels != null && offlineAppRegistrationModels.Any())
            {
                var applicationOfflineHealthItemModels = CreateOfflineApplicationHealthModels(offlineAppRegistrationModels.ToList());

                viewModel.HealthItems.AddRange(applicationOfflineHealthItemModels);
            }

            return this.NegotiateContentResult(viewModel);
        }

        [HttpGet]
        [Route("health/ping")]
        public IActionResult Ping()
        {
            logger.LogInformation($"{nameof(Ping)} has been called");

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

        private async Task<IEnumerable<ApplicationHealthModel>> GetApplicationOnlineHealthAsync(IList<AppRegistrationModel> paths)
        {
            var applicationHealthModels = await CreateApplicationHealthModelTasksAsync(paths);

            // await all application sitemap service tasks to complete
            var allTasks = (from a in applicationHealthModels select a.RetrievalTask).ToArray();

            await Task.WhenAll(allTasks);

            return applicationHealthModels;
        }

        private async Task<List<ApplicationHealthModel>> CreateApplicationHealthModelTasksAsync(IList<AppRegistrationModel> paths)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext) : null;

            var applicationHealthModels = new List<ApplicationHealthModel>();

            foreach (var path in paths)
            {
                logger.LogInformation($"{nameof(Action)}: Getting child Health for: {path.Path}");

                var applicationBaseUrl = await GetPathBaseUrlFromBodyRegionAsync(path.Path);

                var applicationHealthModel = new ApplicationHealthModel
                {
                    Path = path.Path,
                    BearerToken = bearerToken,
                    HealthUrl = $"{applicationBaseUrl}/health",
                };

                applicationHealthModel.RetrievalTask = applicationHealthService.GetAsync(applicationHealthModel);

                applicationHealthModels.Add(applicationHealthModel);
            }

            return applicationHealthModels;
        }

        private async Task<string> GetPathBaseUrlFromBodyRegionAsync(string path)
        {
            var appRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(path);

            var bodyRegion = appRegistrationModel?.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion != null && !string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                var uri = new Uri(bodyRegion.RegionEndpoint);
                var url = $"{uri.Scheme}://{uri.Authority}";

                return url;
            }

            return null;
        }

        private void AppendApplicationsHealths(List<HealthItemViewModel> healthItemModels, IEnumerable<ApplicationHealthModel> applicationHealthModels)
        {
            // get the task results as individual health and merge into one
            foreach (var applicationHealthModel in applicationHealthModels)
            {
                if (applicationHealthModel.RetrievalTask.IsCompletedSuccessfully)
                {
                    logger.LogInformation($"{nameof(Action)}: Received child Health for: {applicationHealthModel.Path}");

                    var healthItems = applicationHealthModel.RetrievalTask.Result;

                    if (healthItems?.Count() > 0)
                    {
                        var healthItemViewModels = (from a in healthItems
                                                    select new HealthItemViewModel
                                                    {
                                                        Service = a.Service,
                                                        Message = a.Message,
                                                    }).ToList();

                        healthItemModels.AddRange(healthItemViewModels);
                    }
                    else
                    {
                        var healthItemViewModel = new HealthItemViewModel
                        {
                            Service = applicationHealthModel.Path,
                            Message = $"No health response from {applicationHealthModel.Path} app",
                        };

                        healthItemModels.Add(healthItemViewModel);
                    }
                }
                else
                {
                    logger.LogError($"{nameof(Action)}: Error getting child Health for: {applicationHealthModel.Path}");
                }
            }
        }

        private List<HealthItemViewModel> CreateOfflineApplicationHealthModels(IList<AppRegistrationModel> paths)
        {
            var healthItemViewModels = new List<HealthItemViewModel>();

            foreach (var path in paths)
            {
                logger.LogInformation($"{nameof(Action)}: Skipping health check for: {path.Path}, because it is offline");

                var healthItemViewModel = new HealthItemViewModel()
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