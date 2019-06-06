using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models.Sitemap;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.Paths;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace DFC.Composite.Shell.Controllers
{
    public class SitemapController : BaseController
    {
        private readonly IPathService _pathService;
        private readonly ILogger<SitemapController> _logger;

        public SitemapController(IPathService pathService, ILogger<SitemapController> logger, IConfiguration configuration) : base(configuration)
        {
            _pathService= pathService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ContentResult> Sitemap()
        {
            try
            {
                _logger.LogInformation("Generating Sitemap");

                var sitemap = GenerateThisSiteSitemap();

                // get all the registered application site maps
                await GetApplicationSitemapsAsync(sitemap);

                string xmlString = sitemap.WriteSitemapToString();

                _logger.LogInformation("Generated Sitemap");

                return Content(xmlString, "application/xml");
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(Sitemap)}: BrokenCircuit: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Sitemap)}: {ex.Message}");
            }

            return null;
        }

        private Sitemap GenerateThisSiteSitemap()
        {
            const string homeControllerName = "Home";
            var sitemap = new Sitemap();

            // output the composite UI site maps
            sitemap.Add(new SitemapLocation() { Url = Url.Action(nameof(HomeController.Index), homeControllerName, null, Request.Scheme), Priority = 1 });

            return sitemap;
        }

        private async Task GetApplicationSitemapsAsync(Sitemap sitemap)
        {
            // loop through the registered applications and create some tasks - one per application that has a sitemap url
            var paths = await _pathService.GetPaths();
            var applicationSitemapServices = await CreateApplicationSitemapServiceTasksAsync(paths);

            // await all application sitemap service tasks to complete
            var allTasks = (from a in applicationSitemapServices select a.TheTask).ToArray();

            await Task.WhenAll(allTasks);

            OutputApplicationsSitemaps(sitemap, paths, applicationSitemapServices);
        }

        private async Task<List<IApplicationSitemapService>> CreateApplicationSitemapServiceTasksAsync(IEnumerable<Models.PathModel> paths)
        {
            // loop through the registered applications and create some tasks - one per application that has a sitemap url
            var applicationSitemapServices = new List<IApplicationSitemapService>();
            string bearerToken = await GetBearerTokenAsync();

            foreach (var path in paths.Where(w => !string.IsNullOrEmpty(w.SitemapURL)))
            {
                var applicationSitemapService = HttpContext.RequestServices.GetService(typeof(IApplicationSitemapService)) as ApplicationSitemapService;

                applicationSitemapService.BearerToken = bearerToken;
                applicationSitemapService.SitemapUrl = path.SitemapURL;
                applicationSitemapService.TheTask = applicationSitemapService.GetAsync();

                applicationSitemapServices.Add(applicationSitemapService);
            }

            return applicationSitemapServices;
        }

        private void OutputApplicationsSitemaps(Sitemap sitemap, IEnumerable<Models.PathModel> paths, List<IApplicationSitemapService> applicationSitemapServices)
        {
            string baseUrl = BaseUrl();

            // get the task results as individual sitemaps and merge into one
            foreach (var applicationSiteMap in applicationSitemapServices)
            {
                if (applicationSiteMap.TheTask.IsCompletedSuccessfully)
                {
                    var mappings = applicationSiteMap.TheTask.Result;

                    if (mappings?.Count() > 0)
                    {
                        foreach (var mapping in mappings)
                        {
                            // rewrite the URL to swap any child application address prefix for the composite UI address prefix
                            foreach (var path in paths)
                            {
                                var pathRootUri = new Uri(path.SitemapURL);
                                string appBaseUrl =$"{pathRootUri.Scheme}://{pathRootUri.Authority}";

                                if (mapping.Url.StartsWith(appBaseUrl, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    mapping.Url = mapping.Url.Replace(appBaseUrl, baseUrl, StringComparison.InvariantCultureIgnoreCase);
                                }
                            }
                        }

                        sitemap.AddRange(mappings);
                    }
                }
            }
        }
    }
}
