using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;
using Microsoft.AspNetCore.Hosting;
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
    public class RobotController : Controller
    {
        private readonly IPathDataService pathDataService;
        private readonly ILogger<RobotController> logger;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IApplicationRobotService applicationRobotService;
        private readonly IShellRobotFileService shellRobotFileService;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IBaseUrlService baseUrlService;

        public RobotController(
            IPathDataService pathDataService,
            ILogger<RobotController> logger,
            IHostingEnvironment hostingEnvironment,
            IBearerTokenRetriever bearerTokenRetriever,
            IApplicationRobotService applicationRobotService,
            IShellRobotFileService shellRobotFileService,
            IBaseUrlService baseUrlService)
        {
            this.pathDataService = pathDataService;
            this.logger = logger;
            this.hostingEnvironment = hostingEnvironment;
            this.bearerTokenRetriever = bearerTokenRetriever;
            this.applicationRobotService = applicationRobotService;
            this.shellRobotFileService = shellRobotFileService;
            this.baseUrlService = baseUrlService;
        }

        [HttpGet]
        public async Task<ContentResult> Robot()
        {
            try
            {
                logger.LogInformation("Generating Robots.txt");

                var robot = new Robot();

                await AppendShellRobot(robot).ConfigureAwait(false);

                // get all the registered application robots.txt
                var applicationRobotModels = await GetApplicationRobotsAsync().ConfigureAwait(false);
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
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(Robot)}: BrokenCircuit: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(Robot)}: {ex.Message}");
            }

            // fall through from errors
            return Content(null, MediaTypeNames.Text.Plain);
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

        private async Task AppendShellRobot(Robot robot)
        {
            var shellRobotsText = await shellRobotFileService.GetFileText(hostingEnvironment.WebRootPath).ConfigureAwait(false);
            robot.Append(shellRobotsText);

            // add any dynamic robots data from the Shell app
        }

        private async Task<IEnumerable<ApplicationRobotModel>> GetApplicationRobotsAsync()
        {
            var paths = await pathDataService.GetPaths().ConfigureAwait(false);
            var onlinePaths = paths.Where(w => w.IsOnline && !string.IsNullOrWhiteSpace(w.RobotsURL)).ToList();

            var applicationRobotModels = await CreateApplicationRobotModelTasksAsync(onlinePaths).ConfigureAwait(false);

            var allRobotRetrievalTasks = (from a in applicationRobotModels select a.RetrievalTask).ToArray();

            await Task.WhenAll(allRobotRetrievalTasks).ConfigureAwait(false);

            return applicationRobotModels;
        }

        private async Task<List<ApplicationRobotModel>> CreateApplicationRobotModelTasksAsync(IEnumerable<PathModel> paths)
        {
            var bearerToken = User.Identity.IsAuthenticated ? await bearerTokenRetriever.GetToken(HttpContext).ConfigureAwait(false) : null;

            var applicationRobotModels = new List<ApplicationRobotModel>();

            foreach (var path in paths)
            {
                logger.LogInformation($"{nameof(Action)}: Getting child robots.txt for: {path.Path}");

                var applicationRobotModel = new ApplicationRobotModel
                {
                    Path = path.Path,
                    RobotsURL = path.RobotsURL,
                    BearerToken = bearerToken,
                };

                applicationRobotModel.RetrievalTask = applicationRobotService.GetAsync(applicationRobotModel);

                applicationRobotModels.Add(applicationRobotModel);
            }

            return applicationRobotModels;
        }

        private void AppendApplicationsRobots(Robot robot, IEnumerable<ApplicationRobotModel> applicationRobotModels)
        {
            var baseUrl = baseUrlService.GetBaseUrl(Request, Url);

            // get the task results as individual robots and merge into one
            foreach (var applicationRobotModel in applicationRobotModels)
            {
                if (applicationRobotModel.RetrievalTask.IsCompletedSuccessfully)
                {
                    logger.LogInformation($"{nameof(Action)}: Received child robots.txt for: {applicationRobotModel.Path}");

                    var applicationRobotsText = applicationRobotModel.RetrievalTask.Result;

                    if (!string.IsNullOrWhiteSpace(applicationRobotsText))
                    {
                        AppendApplicationRobotData(applicationRobotModel, applicationRobotsText, baseUrl, robot);
                    }
                }
                else
                {
                    logger.LogError($"{nameof(Action)}: Error getting child robots.txt for: {applicationRobotModel.Path}");
                }
            }
        }

        private static void AppendApplicationRobotData(ApplicationRobotModel applicationRobotModel, string applicationRobotsText, string baseUrl, Robot robot)
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
    }
}