using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.IntegrationTests.Tests
{
    public class ShellRobotFileServiceTests
    {
        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForDraftDev()
        {
            const string expectedFileText =
@"User-agent: *
Disallow: /";

            var fileInfoHelper = new FileInfoHelper();

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dev-draft.nationalcareersservice.org.uk"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot");
            Assert.Equal(expectedFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForPreProd()
        {
            const string expectedFileText =
@"User-agent: SemrushBot-SA
Disallow: /alerts/
Disallow: /ab/
Disallow: /webchat/
{Insertion}

User-agent: *
Disallow: /";

            var fileInfoHelper = new FileInfoHelper();

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dfc-pp-compui-shell-as-ver2.azurewebsites.net"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot");
            Assert.Equal(expectedFileText, result);
        }

        [Fact]
        public async Task GetFileTextIdentifiesCorrectResponseForProd()
        {
            const string expectedFileText =
@"User-agent: *
Disallow: /alerts/
Disallow: /ab/
Disallow: /webchat/
Disallow: /find-a-course/details*
Disallow: /find-a-course/course-details*
Disallow: /find-a-course/tdetails*
Disallow: /find-a-course/tlevels*
{Insertion}";

            var fileInfoHelper = new FileInfoHelper();

            var httpContextAccessor = A.Fake<IHttpContextAccessor>();
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Host).Returns(new HostString("dfc-prd-compui-shell-as-ver2.azurewebsites.net"));

            var service = new ShellRobotFileService(fileInfoHelper, httpContextAccessor);

            var result = await service.GetStaticFileText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot");
            Assert.Equal(expectedFileText, result);
        }
    }
}
