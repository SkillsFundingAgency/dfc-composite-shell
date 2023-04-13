using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using System;
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
            var service = new ShellRobotFileService(fileInfoHelper, null);

            var result = await service.GetStaticFileText("SomeRobotsPath");
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public async Task GetFileTextReturnsFilesTextWhenFileDoesntExist()
        {
            const string fakeRobotFileText = "FakeRobotFileText";
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileInfoHelper.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileInfoHelper.ReadAllTextAsync(A<string>.Ignored)).Returns(fakeRobotFileText);

            var service = new ShellRobotFileService(fileInfoHelper, null);

            var result = await service.GetStaticFileText("SomeRobotsPath");
            Assert.Equal(fakeRobotFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForDev()
        {
            const string fakeRobotFileText = "StaticRobotsFileText";
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileInfoHelper.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileInfoHelper.ReadAllTextAsync("SomeRobotsPath\\StaticRobots.txt")).Returns(fakeRobotFileText);

            var service = new ShellRobotFileService(fileInfoHelper, null);

            var result = await service.GetStaticFileText("SomeRobotsPath");
            Assert.Equal(fakeRobotFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForDraftDev()
        {
            const string fakeRobotFileText = "StaticRobotsFileText";
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileInfoHelper.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileInfoHelper.ReadAllTextAsync("SomeRobotsPath\\StaticRobots.txt")).Returns(fakeRobotFileText);

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dev-beta.nationalcareersservice.org.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText("SomeRobotsPath");
            Assert.Equal(fakeRobotFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForPP()
        {
            const string fakeRobotFileText = "StaticRobotsFileText";
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileInfoHelper.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileInfoHelper.ReadAllTextAsync("SomeRobotsPath\\StagingStaticRobots.txt")).Returns(fakeRobotFileText);

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dfc-pp-compui-shell-as.ase-01.dfc.preprodazure.sfa.bis.gov.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText("SomeRobotsPath");
            Assert.Equal(fakeRobotFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForProd()
        {
            const string fakeRobotFileText = "StaticRobotsFileText";
            var fileInfoHelper = A.Fake<IFileInfoHelper>();
            A.CallTo(() => fileInfoHelper.FileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => fileInfoHelper.ReadAllTextAsync("SomeRobotsPath\\ProductionStaticRobots.txt")).Returns(fakeRobotFileText);

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dfc-prd-compui-shell-as.ase-01.dfc.prodazure.sfa.bis.gov.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText("SomeRobotsPath");
            Assert.Equal(fakeRobotFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForDraftDevIntegration()
        {
            const string expectedFileText =
@"";

            var fileInfoHelper = new FileInfoHelper();

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dev-draft.nationalcareersservice.org.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot");
            Assert.Equal(expectedFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForPreProdIntegration()
        {
            const string expectedFileText =
@"User-agent: SemrushBot-SA
Disallow: /alerts/
Disallow: /ab/
Disallow: /webchat/
{Insertion}";

            var fileInfoHelper = new FileInfoHelper();

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dfc-pp-compui-shell-as.ase-01.dfc.preprodazure.sfa.bis.gov.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot");
            Assert.Equal(expectedFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForProdIntegration()
        {
            const string expectedFileText =
@"User-agent: *
Disallow: /alerts/
Disallow: /ab/
Disallow: /webchat/
{Insertion}";

            var fileInfoHelper = new FileInfoHelper();

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dfc-prd-compui-shell-as.ase-01.dfc.prodazure.sfa.bis.gov.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot");
            Assert.Equal(expectedFileText, result);
        }
    }
}