using DFC.Composite.Shell.Services.Utilities;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ShellRobotFile
{
    public class ShellRobotFileService : IShellRobotFileService
    {
        private readonly IFileInfoHelper fileInfoHelper;

        public ShellRobotFileService(IFileInfoHelper fileInfoHelper)
        {
            this.fileInfoHelper = fileInfoHelper;
        }

        public async Task<string> GetFileText(string webRootPath)
        {
            var shellRobotsFile = System.IO.Path.Combine(webRootPath, "StaticRobots.txt");
            if (!fileInfoHelper.FileExists(shellRobotsFile))
            {
                return string.Empty;
            }

            var shellRobotsText = await fileInfoHelper.ReadAllTextAsync(shellRobotsFile).ConfigureAwait(false);
            return !string.IsNullOrEmpty(shellRobotsText) ? shellRobotsText : string.Empty;
        }
    }
}