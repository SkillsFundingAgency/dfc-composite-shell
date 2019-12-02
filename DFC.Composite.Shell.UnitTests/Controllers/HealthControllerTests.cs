using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.HealthModels;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Services.TokenRetriever;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class HealthControllerTests
    {
        private readonly HealthController healthController;

        private readonly IPathDataService pathDataService;
        private readonly IRegionService regionService;
        private readonly ILogger<HealthController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IApplicationHealthService applicationHealthService;

        public HealthControllerTests()
        {
            pathDataService = A.Fake<IPathDataService>();
            regionService = A.Fake<IRegionService>();
            logger = A.Fake<ILogger<HealthController>>();
            bearerTokenRetriever = A.Fake<IBearerTokenRetriever>();
            applicationHealthService = A.Fake<IApplicationHealthService>();

            healthController = new HealthController(pathDataService, regionService, logger, bearerTokenRetriever, applicationHealthService);
        }

        [Fact]
        public void PingReturnsSuccess()
        {
            var result = healthController.Ping();

            Assert.IsAssignableFrom<OkResult>(result);
        }

        [Theory]
        [InlineData(MediaTypeNames.Application.Json)]
        [InlineData(MediaTypeNames.Text.Html)]
        public async Task HealthReturnsSuccess(string mediaTypeName)
        {
            //Arrange
            var claims = new List<Claim>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

            healthController.ControllerContext = new ControllerContext();
            healthController.ControllerContext.HttpContext = new DefaultHttpContext() { User = user };
            healthController.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Accept, mediaTypeName);

            var path1 = "path1";
            var path2 = "path2";
            var pathModels = new List<PathModel>
            {
                new PathModel
                {
                    Path = path1,
                    IsOnline = true,
                },
                new PathModel
                {
                    Path = path2,
                    IsOnline = false,
                },
            };

            var regions = new List<RegionModel>()
            {
                new RegionModel()
                {
                    Path = path1,
                    RegionEndpoint = $"http://localhost/{path1}/region1",
                    PageRegion = PageRegion.Body,
                },
                new RegionModel()
                {
                    Path = path2,
                    RegionEndpoint = $"http://localhost/{path2}/region2",
                    PageRegion = PageRegion.Body,
                },
            };

            var path1HealthItemModels = new List<HealthItemModel>();
            path1HealthItemModels.Add(new HealthItemModel() { Message = "Message1", Service = "Service1" });
            path1HealthItemModels.Add(new HealthItemModel() { Message = "Message2", Service = "Service2" });

            A.CallTo(() => pathDataService.GetPaths()).Returns(pathModels);
            A.CallTo(() => regionService.GetRegions(A<string>.Ignored)).Returns(regions);
            A.CallTo(() => applicationHealthService.GetAsync(A<ApplicationHealthModel>.Ignored)).Returns(path1HealthItemModels);

            //Act
            var result = await healthController.Health().ConfigureAwait(false);
            var model = GetModel<HealthViewModel>(result);

            //Assert
            Assert.Equal(4, model.HealthItems.Count);
            Assert.Contains(model.HealthItems, x => x.Message.Contains("Composite Shell is available", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.HealthItems, x => x.Message.Contains("Message1", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.HealthItems, x => x.Message.Contains("Message2", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.HealthItems, x => x.Message.Contains("Skipped health check for: path2, because it is offline", StringComparison.OrdinalIgnoreCase));
        }

        private T GetModel<T>(IActionResult actionResult)
        {
            T result = default;
            var okObjectResult = actionResult as OkObjectResult;
            if (okObjectResult != null)
            {
                result = (T)okObjectResult.Value;
            }
            else
            {
                var viewResult = actionResult as ViewResult;
                if (viewResult != null)
                {
                    result = (T)viewResult.Model;
                }
            }

            return result;
        }
    }
}
