using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.HealthModels;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Services.TokenRetriever;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class HealthController : Controller
    {
        private readonly IPathDataService pathDataService;
        private readonly IRegionService regionService;
        private readonly ILogger<HealthController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IApplicationHealthService applicationHealthService;

        public HealthController(
            IPathDataService pathDataService,
            IRegionService regionService,
            ILogger<HealthController> logger,
            IBearerTokenRetriever bearerTokenRetriever,
            IApplicationHealthService applicationHealthService)
        {
            this.pathDataService = pathDataService;
            this.regionService = regionService;
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

            try
            {
                message = "Composite Shell is available";
                logger.LogInformation($"{nameof(Health)} responded with: {resourceName} - {message}");

                var viewModel = CreateHealthViewModel(resourceName, message);

                // loop through the registered applications and create some tasks - one per application for their health
                var paths = await pathDataService.GetPaths().ConfigureAwait(false);
                var onlinePaths = paths.Where(w => w.IsOnline && string.IsNullOrWhiteSpace(w.ExternalURL)).ToList();
                var offlinePaths = paths.Where(w => !w.IsOnline && string.IsNullOrWhiteSpace(w.ExternalURL)).ToList();

                if (onlinePaths?.Count > 0)
                {
                    var applicationOnlineHealthModels = await GetApplicationOnlineHealthAsync(onlinePaths).ConfigureAwait(false);

                    AppendApplicationsHealths(viewModel.HealthItems, applicationOnlineHealthModels);
                }

                if (offlinePaths?.Count > 0)
                {
                    var applicationOfflineHealthItemModels = CreateOfflineApplicationHealthModels(offlinePaths.ToList());

                    viewModel.HealthItems.AddRange(applicationOfflineHealthItemModels);
                }

                return this.NegotiateContentResult(viewModel);
            }
            catch (Exception ex)
            {
                message = $"{resourceName} exception: {ex.Message}";
                logger.LogError(ex, $"{nameof(Health)}: {message}");
            }

            return StatusCode((int)HttpStatusCode.ServiceUnavailable);
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

        private async Task<IEnumerable<ApplicationHealthModel>> GetApplicationOnlineHealthAsync(IList<Models.PathModel> paths)
        {
            var applicationHealthModels = await CreateApplicationHealthModelTasksAsync(paths).ConfigureAwait(false);

            // await all application sitemap service tasks to complete
            var allTasks = (from a in applicationHealthModels select a.RetrievalTask).ToArray();

            await Task.WhenAll(allTasks).ConfigureAwait(false);

            return applicationHealthModels;
        }

        private async Task<List<ApplicationHealthModel>> CreateApplicationHealthModelTasksAsync(IList<Models.PathModel> paths)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext).ConfigureAwait(false) : null;

            var applicationHealthModels = new List<ApplicationHealthModel>();

            foreach (var path in paths)
            {
                logger.LogInformation($"{nameof(Action)}: Getting child Health for: {path.Path}");

                var applicationBaseUrl = await GetPathBaseUrlFromBodyRegionAsync(path.Path).ConfigureAwait(false);

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
            var regions = await regionService.GetRegions(path).ConfigureAwait(false);

            var bodyRegion = regions?.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

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

        private List<HealthItemViewModel> CreateOfflineApplicationHealthModels(IList<Models.PathModel> paths)
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