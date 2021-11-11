using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.SitemapModels;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.TokenRetriever;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class SitemapController : Controller
    {
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly ILogger<SitemapController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IBaseUrlService baseUrlService;
        private readonly IApplicationSitemapService sitemapService;

        public SitemapController(
            IAppRegistryDataService appRegistryDataService,
            ILogger<SitemapController> logger,
            IBearerTokenRetriever bearerTokenRetriever,
            IBaseUrlService baseUrlService,
            IApplicationSitemapService sitemapService)
        {
            this.appRegistryDataService = appRegistryDataService;
            this.logger = logger;
            this.bearerTokenRetriever = bearerTokenRetriever;
            this.baseUrlService = baseUrlService;
            this.sitemapService = sitemapService;
        }

        [HttpGet]
        public async Task<ContentResult> Sitemap()
        {
            logger.LogInformation("Generating Sitemap.xml");

            var sitemap = new Sitemap();

            // get all the registered application site maps
            var applicationSitemapModels = await GetApplicationSitemapsAsync();
            AppendApplicationsSitemaps(sitemap, applicationSitemapModels);

            var xmlString = sitemap.WriteSitemapToString();

            logger.LogInformation("Generated Sitemap.xml");

            return Content(xmlString, MediaTypeNames.Application.Xml);
        }

        private async Task<IEnumerable<ApplicationSitemapModel>> GetApplicationSitemapsAsync()
        {
            // loop through the registered applications and create some tasks - one per application that has a sitemap url
            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            var onlineAppRegistrationModels = appRegistrationModels.Where(w => w.IsOnline && w.SitemapURL != null).ToList();

            var applicationSitemapModels = await CreateApplicationSitemapModelTasksAsync(onlineAppRegistrationModels);

            // await all application sitemap service tasks to complete
            var allTasks = (from a in applicationSitemapModels select a.RetrievalTask).ToArray();

            await Task.WhenAll(allTasks);

            return applicationSitemapModels;
        }

        private async Task<List<ApplicationSitemapModel>> CreateApplicationSitemapModelTasksAsync(IList<AppRegistrationModel> appRegistrationModels)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext) : null;

            var applicationSitemapModels = new List<ApplicationSitemapModel>();

            foreach (var path in appRegistrationModels)
            {
                logger.LogInformation($"{nameof(Action)}: Getting child Sitemap for: {path.Path}");

                var applicationSitemapModel = new ApplicationSitemapModel
                {
                    Path = path.Path,
                    BearerToken = bearerToken,
                    SitemapUrl = path.SitemapURL.ToString(),
                };

                applicationSitemapModel.RetrievalTask = sitemapService.GetAsync(applicationSitemapModel);

                applicationSitemapModels.Add(applicationSitemapModel);
            }

            return applicationSitemapModels;
        }

        private void AppendApplicationsSitemaps(Sitemap sitemap, IEnumerable<ApplicationSitemapModel> applicationSitemapModels)
        {
            var baseUrl = baseUrlService.GetBaseUrl(Request, Url);

            // get the task results as individual sitemaps and merge into one
            foreach (var applicationSitemapModel in applicationSitemapModels)
            {
                if (applicationSitemapModel.RetrievalTask.IsCompletedSuccessfully)
                {
                    logger.LogInformation($"{nameof(Action)}: Received child Sitemap for: {applicationSitemapModel.Path}");

                    var mappings = applicationSitemapModel.RetrievalTask.Result;

                    var sitemapLocations = mappings?.ToList();
                    if (sitemapLocations == null || !sitemapLocations.Any())
                    {
                        continue;
                    }

                    foreach (var mapping in sitemapLocations)
                    {
                        // rewrite the URL to swap any child application address prefix for the composite UI address prefix
                        var pathRootUri = new Uri(applicationSitemapModel.SitemapUrl);
                        var appBaseUrl = $"{pathRootUri.Scheme}://{pathRootUri.Authority}";

                        if (mapping.Url.StartsWith(appBaseUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            mapping.Url = mapping.Url.Replace(appBaseUrl, baseUrl, StringComparison.InvariantCultureIgnoreCase);
                        }

                        mapping.Priority = 0.5;
                    }

                    sitemap.AddRange(sitemapLocations);
                }
                else
                {
                    logger.LogError($"{nameof(Action)}: Error getting child Sitemap for: {applicationSitemapModel.Path}");
                }
            }
        }
    }
}