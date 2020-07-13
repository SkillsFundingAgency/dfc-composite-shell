using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using FakeItEasy;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class AppRegistryDataServiceTests
    {
        private readonly List<AppRegistrationModel> pathModels;

        public AppRegistryDataServiceTests()
        {
            pathModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    IsOnline = true,
                    Path = "SomeFakePath",
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
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(pathModels);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);
            var result = await appRegistryDataService.GetAppRegistrationModels().ConfigureAwait(false);

            Assert.Equal(pathModels, result);
        }

        [Fact]
        public async Task GetPathReturnsFirstMatchingPathModelResult()
        {
            var fakeAppRegistryService = A.Fake<IAppRegistryService>();
            A.CallTo(() => fakeAppRegistryService.GetPaths()).Returns(pathModels);

            var appRegistryDataService = new AppRegistryDataService(fakeAppRegistryService);
            var result = await appRegistryDataService.GetAppRegistrationModel("SecondFakePath").ConfigureAwait(false);

            Assert.Equal(pathModels[1], result);
        }
    }
}