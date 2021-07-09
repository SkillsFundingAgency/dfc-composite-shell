using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Sitemap;
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
        private readonly IAppRegistryService appRegistryDataService;
        private readonly ILogger<SitemapController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IBaseUrlService baseUrlService;
        private readonly IApplicationSitemapService sitemapService;

        public SitemapController(
            IAppRegistryService appRegistryDataService,
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

            var applicationSitemapModels = await GetAllRegisteredApplicationSitemapsAsync();
            var sitemap = CreateApplicationsSitemap(applicationSitemapModels);

            var xmlString = sitemap.WriteSitemapToString();
            logger.LogInformation("Generated Sitemap.xml");

            return Content(xmlString, MediaTypeNames.Application.Xml);
        }

        private async Task<IEnumerable<ApplicationSitemapModel>> GetAllRegisteredApplicationSitemapsAsync()
        {
            // loop through the registered applications and create some tasks - one per application that has a sitemap url
            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            var onlineAppRegistrationModels = appRegistrationModels.Where(model => model.IsOnline && model.SitemapURL != null).ToList();

            return await CreateApplicationSitemapModelTasksAsync(onlineAppRegistrationModels);
        }

        private async Task<List<ApplicationSitemapModel>> CreateApplicationSitemapModelTasksAsync(
            IList<AppRegistrationModel> appRegistrationModels)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext) : null;
            var modelsTasks = appRegistrationModels.Select(path => GetModelForPath(path, bearerToken)).ToList();

            var applicationSitemapModels = new List<ApplicationSitemapModel>();

            foreach (var modelsTask in modelsTasks)
            {
                applicationSitemapModels.Add(await modelsTask);
            }

            return applicationSitemapModels;
        }

        private async Task<ApplicationSitemapModel> GetModelForPath(AppRegistrationModel path, string bearerToken)
        {
            logger.LogInformation("{action}: Getting child Sitemap for: {path}", nameof(Action), path.Path);

            var applicationSitemapModel = new ApplicationSitemapModel
            {
                Path = path.Path,
                BearerToken = bearerToken,
                SitemapUrl = path.SitemapURL.ToString(),
            };

            return await sitemapService.EnrichAsync(applicationSitemapModel);
        }

        private Sitemap CreateApplicationsSitemap(IEnumerable<ApplicationSitemapModel> applicationSitemapModels)
        {
            var returnObject = new Sitemap();
            var baseUrl = baseUrlService.GetBaseUrl(Request, Url);

            // get the task results as individual sitemaps and merge into one
            foreach (var applicationSitemapModel in applicationSitemapModels)
            {
                logger.LogInformation("{action}: Received child Sitemap for: {path}", nameof(Action), applicationSitemapModel.Path);
                var sitemapLocations = applicationSitemapModel.Data?.ToList();

                if (sitemapLocations.Any() != true)
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
                        mapping.Url = mapping.Url.Replace(appBaseUrl, baseUrl.ToString(), StringComparison.InvariantCultureIgnoreCase);
                    }
                }

                returnObject.AddRange(sitemapLocations);
            }

            return returnObject;
        }
    }
}
