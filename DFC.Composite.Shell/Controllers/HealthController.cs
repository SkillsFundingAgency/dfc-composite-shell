﻿using DFC.Composite.Shell.Extensions;
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
using System.Diagnostics;
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
            var appRegistrationModelsWithBodyRegions = appRegistrationModels.Where(w => w.ExternalURL == null && w.Regions != null && w.Regions.Any(a => a.PageRegion == PageRegion.Body));
            var onlineAppRegistrationModels = appRegistrationModelsWithBodyRegions.Where(w => w.IsOnline).ToList();
            var offlineAppRegistrationModels = appRegistrationModelsWithBodyRegions.Where(w => !w.IsOnline).ToList();

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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                logger.LogInformation($"{nameof(Action)}: Getting child Health for: {path.Path}");

                var applicationBaseUrl = await GetPathBaseUrlFromBodyRegionAsync(path.Path);

                var applicationHealthModel = new ApplicationHealthModel
                {
                    Path = path.Path,
                    BearerToken = bearerToken,
                    HealthUrl = $"{applicationBaseUrl}/health",
                };

                applicationHealthModel.RetrievalTask = applicationHealthService.GetAsync(applicationHealthModel);

                //applicationHealthModel.ResponseTime = stopwatch.ElapsedMilliseconds;

                applicationHealthModels.Add(applicationHealthModel);
            }

            return applicationHealthModels;
        }

        private async Task<string> GetPathBaseUrlFromBodyRegionAsync(string path)
        {
            var appRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(path);

            var bodyRegion = appRegistrationModel?.Regions?.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (!string.IsNullOrWhiteSpace(bodyRegion?.RegionEndpoint))
            {
                var uri = new Uri(bodyRegion.RegionEndpoint);
                var url = $"{uri.Scheme}://{uri.Authority}";

                return url;
            }

            return null;
        }

        private string GetHealthItemHealthMessage(long responseTime)
        {
            if (responseTime == 0)
            {
                return "Unhealthy (" + responseTime + ")";
            }

            if (responseTime < 10000)
            {
                return "Healthy (" + responseTime + ")";
            }

            return "Degraded (" + responseTime + ")";
        }

        private void AppendApplicationsHealths(List<HealthItemViewModel> healthItemModels, IEnumerable<ApplicationHealthModel> applicationHealthModels)
        {
            // get the task results as individual health and merge into one
            foreach (var applicationHealthModel in applicationHealthModels)
            {
                if (applicationHealthModel.RetrievalTask.IsCompletedSuccessfully)
                {
                    var healthItems = applicationHealthModel.RetrievalTask.Result;

                    var healthItemViewModels = (from a in healthItems
                                                    select new HealthItemViewModel
                                                    {
                                                        Service = a.Service,
                                                        Message = $"Received child health for: {applicationHealthModel.Path}: " + GetHealthItemHealthMessage(a.ResponseTime),
                                                    }).ToList();

                    healthItemModels.AddRange(healthItemViewModels);
                }
                else
                {
                    var healthItemViewModel = new HealthItemViewModel
                    {
                        Service = applicationHealthModel.Path,
                        Message = $"Received child health for: {applicationHealthModel.Path}: Unhealthy",
                    };

                    healthItemModels.Add(healthItemViewModel);
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