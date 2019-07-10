using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models.Sitemap;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Utilities;
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

        public SitemapController(IPathService pathService, 
            ILogger<SitemapController> logger, 
            IConfiguration configuration,
            IVersionedFiles versionedFiles)
        : base(configuration, versionedFiles)
        {
            _pathService= pathService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ContentResult> Sitemap()
        {
            try
            {
                _logger.LogInformation("Generating Sitemap.xml");

                var sitemap = GenerateThisSiteSitemap();

                // get all the registered application site maps
                await GetApplicationSitemapsAsync(sitemap);

                string xmlString = sitemap.WriteSitemapToString();

                _logger.LogInformation("Generated Sitemap.xml");

                return Content(xmlString, MediaTypeNames.Application.Xml);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(Sitemap)}: BrokenCircuit: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Sitemap)}: {ex.Message}");
            }

            // fall through from errors
            return Content(null, MediaTypeNames.Application.Xml);
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
            var onlinePaths = paths.Where(w => w.IsOnline && !string.IsNullOrEmpty(w.SitemapURL)).ToList();

            var applicationSitemapServices = await CreateApplicationSitemapServiceTasksAsync(onlinePaths);

            // await all application sitemap service tasks to complete
            var allTasks = (from a in applicationSitemapServices select a.TheTask).ToArray();

            await Task.WhenAll(allTasks);

            OutputApplicationsSitemaps(sitemap, onlinePaths, applicationSitemapServices);
        }

        private async Task<List<IApplicationSitemapService>> CreateApplicationSitemapServiceTasksAsync(IList<Models.PathModel> paths)
        {
            // loop through the registered applications and create some tasks - one per application that has a sitemap url
            var applicationSitemapServices = new List<IApplicationSitemapService>();
            string bearerToken = await GetBearerTokenAsync();

            foreach (var path in paths)
            {
                _logger.LogInformation($"{nameof(Action)}: Getting child Sitemap for: {path.Path}");

                var applicationSitemapService = HttpContext.RequestServices.GetService(typeof(IApplicationSitemapService)) as ApplicationSitemapService;

                applicationSitemapService.Path = path.Path;
                applicationSitemapService.BearerToken = bearerToken;
                applicationSitemapService.SitemapUrl = path.SitemapURL;
                applicationSitemapService.TheTask = applicationSitemapService.GetAsync();

                applicationSitemapServices.Add(applicationSitemapService);
            }

            return applicationSitemapServices;
        }

        private void OutputApplicationsSitemaps(Sitemap sitemap, IList<Models.PathModel> paths, List<IApplicationSitemapService> applicationSitemapServices)
        {
            string baseUrl = BaseUrl();

            // get the task results as individual sitemaps and merge into one
            foreach (var applicationSitemapService in applicationSitemapServices)
            {
                if (applicationSitemapService.TheTask.IsCompletedSuccessfully)
                {
                    _logger.LogInformation($"{nameof(Action)}: Received child Sitemap for: {applicationSitemapService.Path}");

                    var mappings = applicationSitemapService.TheTask.Result;

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
                else
                {
                    _logger.LogError($"{nameof(Action)}: Error getting child Sitemap for: {applicationSitemapService.Path}");
                }
            }
        }
    }
}
