using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.Utilities;
using FakeItEasy;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class ShellRobotFileServiceTests
    {
        [Fact]
        public async Task GetFileTextReturnsEmptyStringWhenFileDoesntExist()
        {
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            var service = new ShellRobotFileService(fileInfoHelper);

            var result = await service.GetFileText("SomeRobotsPath").ConfigureAwait(false);
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public async Task GetFileTextReturnsFilesTextWhenFileDoesntExist()
        {
            const string fakeRobotFileText = "FakeRobotFileText";
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileInfoHelper.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileInfoHelper.ReadAllTextAsync(A<string>.Ignored)).Returns(fakeRobotFileText);

            var service = new ShellRobotFileService(fileInfoHelper);

            var result = await service.GetFileText("SomeRobotsPath").ConfigureAwait(false);
            Assert.Equal(fakeRobotFileText, result);
        }
    }
}