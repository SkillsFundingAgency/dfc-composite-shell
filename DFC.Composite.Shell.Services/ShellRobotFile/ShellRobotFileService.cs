using DFC.Composite.Shell.Services.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ShellRobotFile
{
    public class ShellRobotFileService : IShellRobotFileService
    {
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

        private static bool IsDraft(string hostname)
        {
            const string draft = "draft";
            return hostname.Contains(draft, System.StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsPreProduction(string hostname)
        {
            const string stagingHostname = "dfc-pp-compui-shell-as.ase-01.dfc.preprodazure.sfa.bis.gov.uk";
            return hostname.Equals(stagingHostname, System.StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsProduction(string hostname)
        {
            const string productionHostname = "dfc-prd-compui-shell-as.ase-01.dfc.prodazure.sfa.bis.gov.uk";
            return hostname.Equals(productionHostname, System.StringComparison.InvariantCultureIgnoreCase);
        }

        private string StaticRobotsFilename()
        {
            const string standardRobotsFilename = "StaticRobots.txt";
            var hostname = httpContextAccessor?.HttpContext?.Request?.Host.Host ?? string.Empty;

            if (IsDraft(hostname))
            {
                return standardRobotsFilename;
            }

            if (IsPreProduction(hostname))
            {
                const string stagingRobotsFilename = "StagingStaticRobots.txt";
                return stagingRobotsFilename;
            }

            if (IsProduction(hostname))
            {
                const string productionRobotsFilename = "ProductionStaticRobots.txt";
                return productionRobotsFilename;
            }

            return standardRobotsFilename;
        }
    }
}