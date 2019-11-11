using DFC.Composite.Shell.Models.SitemapModels;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.TokenRetriever;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class SitemapController : Controller
    {
        private readonly IPathDataService pathDataService;
        private readonly ILogger<SitemapController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IBaseUrlService baseUrlService;
        private readonly IApplicationSitemapService sitemapService;

        public SitemapController(
            IPathDataService pathDataService,
            ILogger<SitemapController> logger,
            IBearerTokenRetriever bearerTokenRetriever,
            IBaseUrlService baseUrlService,
            IApplicationSitemapService sitemapService)
        {
            this.pathDataService = pathDataService;
            this.logger = logger;
            this.bearerTokenRetriever = bearerTokenRetriever;
            this.baseUrlService = baseUrlService;
            this.sitemapService = sitemapService;
        }

        [HttpGet]
        public async Task<ContentResult> Sitemap()
        {
            try
            {
                logger.LogInformation("Generating Sitemap.xml");

                var sitemap = AppendShellSitemap();

                // get all the registered application site maps
                var applicationSitemapModels = await GetApplicationSitemapsAsync().ConfigureAwait(false);
                AppendApplicationsSitemaps(sitemap, applicationSitemapModels);

                var xmlString = sitemap.WriteSitemapToString();

                logger.LogInformation("Generated Sitemap.xml");

                return Content(xmlString, MediaTypeNames.Application.Xml);
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(Sitemap)}: BrokenCircuit: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(Sitemap)}: {ex.Message}");
            }

            // fall through from errors
            return Content(null, MediaTypeNames.Application.Xml);
        }

        private Sitemap AppendShellSitemap()
        {
            const string homeControllerName = "Home";
            var sitemap = new Sitemap();

            // output the composite UI site maps
            sitemap.Add(new SitemapLocation { Url = Url.Action(nameof(HomeController.Index), homeControllerName, null, Request.Scheme), Priority = 1 });

            return sitemap;
        }

        private async Task<IEnumerable<ApplicationSitemapModel>> GetApplicationSitemapsAsync()
        {
            // loop through the registered applications and create some tasks - one per application that has a sitemap url
            var paths = await pathDataService.GetPaths().ConfigureAwait(false);
            var onlinePaths = paths.Where(w => w.IsOnline && !string.IsNullOrEmpty(w.SitemapURL)).ToList();

            var applicationSitemapModels = await CreateApplicationSitemapModelTasksAsync(onlinePaths).ConfigureAwait(false);

            // await all application sitemap service tasks to complete
            var allTasks = (from a in applicationSitemapModels select a.RetrievalTask).ToArray();

            await Task.WhenAll(allTasks).ConfigureAwait(false);

            return applicationSitemapModels;
        }

        private async Task<List<ApplicationSitemapModel>> CreateApplicationSitemapModelTasksAsync(IList<Models.PathModel> paths)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext).ConfigureAwait(false) : null;

            var applicationSitemapModels = new List<ApplicationSitemapModel>();

            foreach (var path in paths)
            {
                logger.LogInformation($"{nameof(Action)}: Getting child Sitemap for: {path.Path}");

                var applicationSitemapModel = new ApplicationSitemapModel
                {
                    Path = path.Path,
                    BearerToken = bearerToken,
                    SitemapUrl = path.SitemapURL,
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