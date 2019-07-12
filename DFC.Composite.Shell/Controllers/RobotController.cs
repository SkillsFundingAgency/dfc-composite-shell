using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models.Robots;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace DFC.Composite.Shell.Controllers
{
    public class RobotController : BaseController
    {
        private readonly IPathDataService _pathDataService;
        private readonly ILogger<RobotController> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public RobotController(
            IPathDataService pathDataService,
            ILogger<RobotController> logger,
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            IVersionedFiles versionedFiles)
        : base(configuration, versionedFiles)
        {
            _pathDataService = pathDataService;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<ContentResult> Robot()
        {
            try
            {
                _logger.LogInformation("Generating Robots.txt");

                var robot = GenerateThisSiteRobot();

                // get all the registered application robots.txt
                await GetApplicationRobotsAsync(robot);

                // add the Shell sitemap route
                string sitemapRouteUrl = Url.RouteUrl("Sitemap", null);

                if (sitemapRouteUrl != null)
                {
                    string shellSitemapUrl = $"{Request.Scheme}://{Request.Host}{sitemapRouteUrl}";

                    robot.Add($"Sitemap: {shellSitemapUrl}");
                }

                _logger.LogInformation("Generated Robots.txt");

                return Content(robot.Data, MediaTypeNames.Text.Plain);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(Robot)}: BrokenCircuit: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Robot)}: {ex.Message}");
            }

            // fall through from errors
            return Content(null, MediaTypeNames.Text.Plain);
        }

        private Robot GenerateThisSiteRobot()
        {
            var robot = new Robot();
            string robotsFilePath = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "StaticRobots.txt");

            if (System.IO.File.Exists(robotsFilePath))
            {
                // output the composite UI default (static) robots data from the StaticRobots.txt file
                string staticRobotsText = System.IO.File.ReadAllText(robotsFilePath);

                if (!string.IsNullOrEmpty(staticRobotsText))
                {
                    robot.Add(staticRobotsText);
                }
            }

            // add any dynamic robots data form the Shell app
            //robot.Add("<<add any dynamic text or other here>>");

            return robot;
        }

        private async Task GetApplicationRobotsAsync(Robot robot)
        {
            // loop through the registered applications and create some tasks - one per application that has a robot url
            var paths = await _pathDataService.GetPaths();
            var onlinePaths = paths.Where(w => w.IsOnline && !string.IsNullOrEmpty(w.RobotsURL)).ToList();

            var applicationRobotServices = await CreateApplicationRobotServiceTasksAsync(onlinePaths);

            // await all application robot service tasks to complete
            var allTasks = (from a in applicationRobotServices select a.TheTask).ToArray();

            await Task.WhenAll(allTasks);

            OutputApplicationsRobots(robot, onlinePaths, applicationRobotServices);
        }

        private async Task<List<IApplicationRobotService>> CreateApplicationRobotServiceTasksAsync(IList<Models.PathModel> paths)
        {
            // loop through the registered applications and create some tasks - one per application that has a robot url
            var applicationRobotServices = new List<IApplicationRobotService>();
            string bearerToken = await GetBearerTokenAsync();


            foreach (var path in paths)
            {
                _logger.LogInformation($"{nameof(Action)}: Getting child robots.txt for: {path.Path}");

                var applicationRobotService = HttpContext.RequestServices.GetService(typeof(IApplicationRobotService)) as ApplicationRobotService;

                applicationRobotService.Path = path.Path;
                applicationRobotService.BearerToken = bearerToken;
                applicationRobotService.RobotsURL = path.RobotsURL;
                applicationRobotService.TheTask = applicationRobotService.GetAsync();

                applicationRobotServices.Add(applicationRobotService);
            }

            return applicationRobotServices;
        }

        private void OutputApplicationsRobots(Robot robot, IList<Models.PathModel> paths, List<IApplicationRobotService> applicationRobotServices)
        {
            string baseUrl = BaseUrl();

            // get the task results as individual sitemaps and merge into one
            foreach (var applicationRobotService in applicationRobotServices)
            {
                if (applicationRobotService.TheTask.IsCompletedSuccessfully)
                {
                    _logger.LogInformation($"{nameof(Action)}: Received child robots.txt for: {applicationRobotService.Path}");

                    var applicationRobotsText = applicationRobotService.TheTask.Result;

                    if (!string.IsNullOrEmpty(applicationRobotsText))
                    {
                        var robotsLines = applicationRobotsText.Split(Environment.NewLine);

                        for (int i = 0; i < robotsLines.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(robotsLines[i]))
                            {
                                // remove any user-agent and sitemap lines
                                var lineSegments = robotsLines[i].Split(":");
                                var skipLinesWithSegment = new string[] { "User-agent", "Sitemap" };

                                if (lineSegments.Length > 0)
                                {
                                    if (skipLinesWithSegment.Contains(lineSegments[0], StringComparer.OrdinalIgnoreCase))
                                    {
                                        robotsLines[i] = null;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(robotsLines[i]))
                            {
                                // rewrite the URL to swap any child application address prefix for the composite UI address prefix
                                foreach (var path in paths)
                                {
                                    var pathRootUri = new Uri(path.RobotsURL);
                                    string appBaseUrl = $"{pathRootUri.Scheme}://{pathRootUri.Authority}";

                                    if (robotsLines[i].StartsWith(appBaseUrl, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        robotsLines[i] = robotsLines[i].Replace(appBaseUrl, baseUrl, StringComparison.InvariantCultureIgnoreCase);
                                    }
                                }
                            }
                        }

                        robot.Add(string.Join(Environment.NewLine, robotsLines.Where(w => !string.IsNullOrEmpty(w))));
                    }
                }
                else
                {
                    _logger.LogError($"{nameof(Action)}: Error getting child robots.txt for: {applicationRobotService.Path}");
                }
            }
        }
    }
}
