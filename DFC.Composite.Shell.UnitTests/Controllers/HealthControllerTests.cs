using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using DFC.Composite.Shell.Models.Health;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.TokenRetriever;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.Controllers
{
    public class HealthControllerTests
    {
        private readonly HealthController healthController;

        private readonly IAppRegistryService appRegistryDataService;
        private readonly ILogger<HealthController> logger;
        private readonly IBearerTokenRetriever bearerTokenRetriever;
        private readonly IApplicationHealthService applicationHealthService;

        public HealthControllerTests()
        {
            appRegistryDataService = A.Fake<IAppRegistryService>();
            logger = A.Fake<ILogger<HealthController>>();
            bearerTokenRetriever = A.Fake<IBearerTokenRetriever>();
            applicationHealthService = A.Fake<IApplicationHealthService>();

            healthController = new HealthController(appRegistryDataService, logger, bearerTokenRetriever, applicationHealthService);
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

            healthController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user },
            };
            healthController.ControllerContext.HttpContext.Request.Headers.Add(HeaderNames.Accept, mediaTypeName);

            const string path1 = "path1";
            const string path2 = "path2";
            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    Path = path1,
                    IsOnline = true,
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            RegionEndpoint = $"http://localhost/{path1}/region1",
                            PageRegion = PageRegion.Body,
                        },
                    },
                },
                new AppRegistrationModel
                {
                    Path = path2,
                    IsOnline = false,
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            RegionEndpoint = $"http://localhost/{path2}/region2",
                            PageRegion = PageRegion.Body,
                        },
                    },
                },
            };

            var healthModel = new ApplicationHealthModel
            {
                Data = new List<HealthItemModel>
                {
                    new HealthItemModel { Message = "Message1", Service = "Service1" },
                    new HealthItemModel { Message = "Message2", Service = "Service2" },
                },
            };

            A.CallTo(() => appRegistryDataService.GetAppRegistrationModels()).Returns(appRegistrationModels);
            A.CallTo(() => appRegistryDataService.GetAppRegistrationModel(A<string>.Ignored)).Returns(appRegistrationModels.FirstOrDefault(f => f.IsOnline));
            A.CallTo(() => applicationHealthService.EnrichAsync(A<ApplicationHealthModel>.Ignored)).Returns(healthModel);

            //Act
            var result = await healthController.Health();
            var model = GetModel<HealthViewModel>(result);

            //Assert
            Assert.Equal(4, model.HealthItems.Count);
            Assert.Contains(model.HealthItems, healthItem => healthItem.Message.Contains("Composite Shell is available", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.HealthItems, healthItem => healthItem.Message.Contains("Message1", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.HealthItems, healthItem => healthItem.Message.Contains("Message2", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(model.HealthItems, healthItem => healthItem.Message.Contains("Skipped health check for: path2, because it is offline", StringComparison.OrdinalIgnoreCase));
        }

        private T GetModel<T>(IActionResult actionResult)
        {
            T result = default;
            if (actionResult is OkObjectResult okObjectResult)
            {
                result = (T)okObjectResult.Value;
            }
            else
            {
                if (actionResult is ViewResult viewResult)
                {
                    result = (T)viewResult.Model;
                }
            }

            return result;
        }
    }
}
