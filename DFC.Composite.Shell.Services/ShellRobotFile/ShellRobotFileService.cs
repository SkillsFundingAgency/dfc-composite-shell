using DFC.Composite.Shell.Services.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ShellRobotFile
{
    public class ShellRobotFileService : IShellRobotFileService
    {
        private const string ProductionEnvHostname = "dfc-prd-compui-shell-as-ver2.azurewebsites.net";
        private const string ProductionEnvRobotFilename = "ProductionStaticRobots.txt";
        private const string NonProductionEnvRobotFilename = "StaticRobots.txt";
        private readonly IFileInfoHelper fileInfoHelper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ShellRobotFileService(IFileInfoHelper fileInfoHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.fileInfoHelper = fileInfoHelper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetStaticFileText(string webRootPath)
        {
            var staticRobotsFile = System.IO.Path.Combine(webRootPath, StaticRobotsFilename());
            if (!fileInfoHelper.FileExists(staticRobotsFile))
            {
                return string.Empty;
            }

            var shellRobotsText = await fileInfoHelper.ReadAllTextAsync(staticRobotsFile);
            return !string.IsNullOrWhiteSpace(shellRobotsText) ? shellRobotsText : string.Empty;
        }

        private string StaticRobotsFilename()
        {
            string hostname = httpContextAccessor?.HttpContext?.Request?.Host.Host ?? string.Empty;
            bool environmentIsProduction = hostname.Equals(ProductionEnvHostname, System.StringComparison.InvariantCultureIgnoreCase);

            if (environmentIsProduction)
            {
                return ProductionEnvRobotFilename;
            }

            return NonProductionEnvRobotFilename;
        }
    }
}