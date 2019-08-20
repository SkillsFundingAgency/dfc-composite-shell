using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Paths;
using FakeItEasy;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class PathDataServiceTests
    {
        private readonly List<PathModel> pathModels;

        public PathDataServiceTests()
        {
            pathModels = new List<PathModel>
            {
                new PathModel
                {
                    IsOnline = true,
                    Path = "SomeFakePath",
                },
                new PathModel
                {
                    IsOnline = true,
                    Path = "SecondFakePath",
                },
                new PathModel
                {
                    IsOnline = true,
                    Path = "ThirdFakePath",
                },
            };
        }

        [Fact]
        public async Task GetPathsReturnsPathModelResults()
        {
            var fakePathService = A.Fake<IPathService>();
            A.CallTo(() => fakePathService.GetPaths()).Returns(pathModels);

            var pathDataService = new PathDataService(fakePathService);
            var result = await pathDataService.GetPaths().ConfigureAwait(false);

            Assert.Equal(pathModels, result);
        }

        [Fact]
        public async Task GetPathReturnsFirstMatchingPathModelResult()
        {
            var fakePathService = A.Fake<IPathService>();
            A.CallTo(() => fakePathService.GetPaths()).Returns(pathModels);

            var pathDataService = new PathDataService(fakePathService);
            var result = await pathDataService.GetPath("SecondFakePath").ConfigureAwait(false);

            Assert.Equal(pathModels[1], result);
        }


    }
}