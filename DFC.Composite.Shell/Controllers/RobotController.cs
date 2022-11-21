using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class RobotController : Controller
    {
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly ILogger<RobotController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IApplicationRobotService applicationRobotService;
        private readonly IShellRobotFileService shellRobotFileService;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IBaseUrlService baseUrlService;

        public RobotController(
            IAppRegistryDataService appRegistryDataService,
            ILogger<RobotController> logger,
            IWebHostEnvironment webHostEnvironment,
            IBearerTokenRetriever bearerTokenRetriever,
            IApplicationRobotService applicationRobotService,
            IShellRobotFileService shellRobotFileService,
            IBaseUrlService baseUrlService)
        {
            this.appRegistryDataService = appRegistryDataService;
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
            this.bearerTokenRetriever = bearerTokenRetriever;
            this.applicationRobotService = applicationRobotService;
            this.shellRobotFileService = shellRobotFileService;
            this.baseUrlService = baseUrlService;
        }

        [HttpGet]
        public async Task<ContentResult> Robot()
        {
            logger.LogInformation("Generating Robots.txt");

            var staticText = await GetStaticContent();

            // get all the registered application robots.txt
            var applicationRobotModels = await GetApplicationRobotsAsync();

            var dynamicText = new StringBuilder();
            AppendApplicationsRobots(dynamicText, applicationRobotModels);

            // add the Shell sitemap route to the bottom
            var sitemapRouteUrl = Url.RouteUrl("sitemap", null).ToLower(CultureInfo.CurrentCulture);

            if (sitemapRouteUrl != null)
            {
                var baseUrl = $"{Request.GetBaseAddress()}".TrimEnd('/');
                dynamicText.Append($"Sitemap: {baseUrl}{sitemapRouteUrl}");
            }

            var combinedText = staticText.Replace("{Insertion}", dynamicText.ToString());
            var robot = new Robot(combinedText);

            logger.LogInformation("Generated Robots.txt");

            return Content(robot.Data, MediaTypeNames.Text.Plain);
        }

        private static IEnumerable<string> ProcessRobotsLines(ApplicationRobotModel applicationRobotModel, string baseUrl, string[] robotsLines)
        {
            for (var i = 0; i < robotsLines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(robotsLines[i]))
                {
                    // remove any user-agent and sitemap lines
                    var lineSegments = robotsLines[i].Split(new[] { ':' }, 2);
                    var skipLinesWithSegment = new[] { "User-agent", "Sitemap" };

                    if (lineSegments.Length > 0 && skipLinesWithSegment.Contains(lineSegments[0], StringComparer.OrdinalIgnoreCase))
                    {
                        robotsLines[i] = string.Empty;
                    }

                    // rewrite the URL to swap any child application address prefix for the composite UI address prefix
                    var pathRootUri = new Uri(applicationRobotModel.RobotsURL);
                    var appBaseUrl = $"{pathRootUri.Scheme}://{pathRootUri.Authority}";

                    if (robotsLines[i].Contains(appBaseUrl, StringComparison.InvariantCultureIgnoreCase))
                    {
                        robotsLines[i] = robotsLines[i].Replace(appBaseUrl, baseUrl, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }

            return robotsLines.Where(w => !string.IsNullOrWhiteSpace(w));
        }

        private static void AppendApplicationRobotData(ApplicationRobotModel applicationRobotModel, string applicationRobotsText, string baseUrl, StringBuilder stringBuilder)
        {
            var robotsLines = applicationRobotsText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            var robotResults = ProcessRobotsLines(applicationRobotModel, baseUrl, robotsLines);

            foreach (var robotResult in robotResults)
            {
                if (!stringBuilder.ToString().Contains(robotResult))
                {
                    stringBuilder.AppendLine(robotResult);
                }
            }
        }

        private Task<string> GetStaticContent()
        {
            return shellRobotFileService.GetStaticFileText(webHostEnvironment.WebRootPath);
        }

        private async Task<IEnumerable<ApplicationRobotModel>> GetApplicationRobotsAsync()
        {
            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            var onlineAppRegistrationModels = appRegistrationModels.Where(w => w.IsOnline && w.RobotsURL != null).ToList();

            var applicationRobotModels = await CreateApplicationRobotModelTasksAsync(onlineAppRegistrationModels);

            var allRobotRetrievalTasks = (from a in applicationRobotModels select a.RetrievalTask).ToArray();

            await Task.WhenAll(allRobotRetrievalTasks);

            return applicationRobotModels;
        }

        private async Task<List<ApplicationRobotModel>> CreateApplicationRobotModelTasksAsync(IEnumerable<AppRegistrationModel> appRegistrationModel)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext) : null;

            var applicationRobotModels = new List<ApplicationRobotModel>();

            foreach (var path in appRegistrationModel)
            {
                logger.LogInformation($"{nameof(Action)}: Getting child robots.txt for: {path.Path}");

                var applicationRobotModel = new ApplicationRobotModel
                {
                    Path = path.Path,
                    RobotsURL = path.RobotsURL.ToString(),
                    BearerToken = bearerToken,
                };

                applicationRobotModel.RetrievalTask = applicationRobotService.GetAsync(applicationRobotModel);

                applicationRobotModels.Add(applicationRobotModel);
            }

            return applicationRobotModels;
        }

        private void AppendApplicationsRobots(StringBuilder stringBuilder, IEnumerable<ApplicationRobotModel> applicationRobotModels)
        {
            var baseUrl = $"{Request.GetBaseAddress()}".TrimEnd('/');

            // get the task results as individual robots and merge into one
            foreach (var applicationRobotModel in applicationRobotModels)
            {
                if (applicationRobotModel.RetrievalTask.IsCompletedSuccessfully)
                {
                    logger.LogInformation($"{nameof(Action)}: Received child robots.txt for: {applicationRobotModel.Path}");

                    var applicationRobotsText = applicationRobotModel.RetrievalTask.Result;

                    if (!string.IsNullOrWhiteSpace(applicationRobotsText))
                    {
                        AppendApplicationRobotData(applicationRobotModel, applicationRobotsText, baseUrl, stringBuilder);
                    }
                }
                else
                {
                    logger.LogError($"{nameof(Action)}: Error getting child robots.txt for: {applicationRobotModel.Path}");
                }
            }
        }
    }
}