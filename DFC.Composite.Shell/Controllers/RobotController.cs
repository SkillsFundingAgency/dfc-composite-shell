using DFC.Composite.Shell.Models.AppRegistration;
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
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class RobotController : Controller
    {
        private readonly IAppRegistryService appRegistryDataService;
        private readonly ILogger<RobotController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IApplicationRobotService applicationRobotService;
        private readonly IShellRobotFileService shellRobotFileService;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IBaseUrlService baseUrlService;

        public RobotController(
            IAppRegistryService appRegistryDataService,
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

            var robot = new Robot();
            await AppendShellRobot(robot);

            // get all the registered application robots.txt
            var applicationRobotModels = await GetApplicationRobotsAsync();
            AppendApplicationsRobots(robot, applicationRobotModels);

            // add the Shell sitemap route to the bottom
            var sitemapRouteUrl = Url.RouteUrl("Sitemap", null);

            if (sitemapRouteUrl != null)
            {
                robot.Add($"Sitemap: {Request.Scheme}://{Request.Host}{sitemapRouteUrl}");
            }

            logger.LogInformation("Generated Robots.txt");

            return Content(robot.Data, MediaTypeNames.Text.Plain);
        }

        private static IEnumerable<string> ProcessRobotsLines(ApplicationRobotModel applicationRobotModel, Uri baseUrl, string[] robotsLines)
        {
            for (var i = 0; i < robotsLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(robotsLines[i]))
                {
                    continue;
                }

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
                    robotsLines[i] = robotsLines[i].Replace(appBaseUrl, baseUrl.ToString(), StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return robotsLines.Where(line => !string.IsNullOrWhiteSpace(line));
        }

        private static void AppendApplicationRobotData(
            ApplicationRobotModel applicationRobotModel,
            string applicationRobotsText,
            Uri baseUrl,
            Robot robot)
        {
            var robotsLines = applicationRobotsText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var robotResults = ProcessRobotsLines(applicationRobotModel, baseUrl, robotsLines);

            foreach (var robotResult in robotResults)
            {
                if (!robot.Lines.Contains(robotResult))
                {
                    robot.Add(robotResult);
                }
            }
        }

        private async Task AppendShellRobot(Robot robot)
        {
            var shellRobotsText = await shellRobotFileService.GetFileText(webHostEnvironment.WebRootPath);
            robot.Append(shellRobotsText);

            // add any dynamic robots data from the Shell app
        }

        private async Task<IEnumerable<ApplicationRobotModel>> GetApplicationRobotsAsync()
        {
            var appRegistrationModels = await appRegistryDataService.GetAppRegistrationModels();
            var onlineAppRegistrationModels = appRegistrationModels.Where(model => model.IsOnline && model.RobotsURL != null).ToList();

            return await CreateApplicationRobotModelTasksAsync(onlineAppRegistrationModels);
        }

        private async Task<List<ApplicationRobotModel>> CreateApplicationRobotModelTasksAsync(
            IEnumerable<AppRegistrationModel> appRegistrationModel)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext) : null;
            var modelsTasks = appRegistrationModel.Select(path => GetModelForPath(path, bearerToken)).ToList();

            var applicationRobotModels = new List<ApplicationRobotModel>();

            foreach (var modelsTask in modelsTasks)
            {
                applicationRobotModels.Add(await modelsTask);
            }

            return applicationRobotModels;
        }

        private async Task<ApplicationRobotModel> GetModelForPath(AppRegistrationModel path, string bearerToken)
        {
            logger.LogInformation("{action}: Getting child robots.txt for: {path}", nameof(Action), path.Path);

            var applicationRobotModel = new ApplicationRobotModel
            {
                Path = path.Path,
                RobotsURL = path.RobotsURL.ToString(),
                BearerToken = bearerToken,
            };

            return await applicationRobotService.EnrichAsync(applicationRobotModel);
        }

        private void AppendApplicationsRobots(Robot robot, IEnumerable<ApplicationRobotModel> applicationRobotModels)
        {
            var baseUrl = baseUrlService.GetBaseUrl(Request, Url);

            // get the task results as individual robots and merge into one
            foreach (var applicationRobotModel in applicationRobotModels)
            {
                logger.LogInformation("{action}: Received child robots.txt for: {path}", nameof(Action), applicationRobotModel.Path);
                var applicationRobotsText = applicationRobotModel.Data;

                if (!string.IsNullOrWhiteSpace(applicationRobotsText))
                {
                    AppendApplicationRobotData(applicationRobotModel, applicationRobotsText, baseUrl, robot);
                }
            }
        }
    }
}
