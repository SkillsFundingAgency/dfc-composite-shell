using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using FakeItEasy;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class AppRegistryDataServiceTests
    {
        private readonly List<AppRegistrationModel> appRegistrationModels;

        public AppRegistryDataServiceTests()
        {
            appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "SomeFakePath",
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            IsHealthy = false,
                        },
                    },
                    AjaxRequests = new List<AjaxRequestModel>
                    {
                        new AjaxRequestModel
                        {
                            Name = "a-valid-name",
                            IsHealthy = false,
                        },
                    },
                },
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "SecondFakePath",
                },
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "ThirdFakePath",
                },
            };
        }

        [Fact]
        public async Task GetPathsReturnsPathModelResults()
        {
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(appRegistrationModels);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);
            var result = await appRegistryDataService.GetAppRegistrationModels().ConfigureAwait(false);

            Assert.Equal(appRegistrationModels, result);
        }

        [Fact]
        public async Task GetPathReturnsFirstMatchingPathModelResult()
        {
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(appRegistrationModels);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);
            var result = await appRegistryDataService.GetAppRegistrationModel("SecondFakePath").ConfigureAwait(false);

            Assert.Equal(appRegistrationModels[1], result);
        }

        [Fact]
        public async Task SetRegionHealthStateForValidRegionSuccess()
        {
            // Arrange
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(appRegistrationModels);
            A.CallTo(() => fakeAppRegistryService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, A<bool>.Ignored)).Returns(true);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);

            // Act
            await appRegistryDataService.SetRegionHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().Regions.First().PageRegion, true).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeAppRegistryService.GetPaths()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAppRegistryService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SetRegionHealthStateForInvalidRegionSuccess()
        {
            // Arrange
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(appRegistrationModels);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);

            // Act
            await appRegistryDataService.SetRegionHealthState(appRegistrationModels.First().Path, PageRegion.Head, true).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeAppRegistryService.GetPaths()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAppRegistryService.SetRegionHealthState(A<string>.Ignored, A<PageRegion>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task SetAjaxRequestHealthStateForValidAjaxRequestSuccess()
        {
            // Arrange
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(appRegistrationModels);
            A.CallTo(() => fakeAppRegistryService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).Returns(true);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);

            // Act
            await appRegistryDataService.SetAjaxRequestHealthState(appRegistrationModels.First().Path, appRegistrationModels.First().AjaxRequests.First().Name, true).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeAppRegistryService.GetPaths()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAppRegistryService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SetAjaxRequestHealthStateForInvalidAjaxRequestSuccess()
        {
            // Arrange
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(appRegistrationModels);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);

            // Act
            await appRegistryDataService.SetAjaxRequestHealthState(appRegistrationModels.First().Path, "unknown", true).ConfigureAwait(false);

            // Assert
            A.CallTo(() => fakeAppRegistryService.GetPaths()).MustHaveHappenedOnceExactly();
            A.CallTo(() => fakeAppRegistryService.SetAjaxRequestHealthState(A<string>.Ignored, A<string>.Ignored, A<bool>.Ignored)).MustNotHaveHappened();
        }
    }
}