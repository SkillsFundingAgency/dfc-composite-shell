using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ShellRobotFile
{
    public class ShellRobotFileService : IShellRobotFileService
    {
        public async Task<string> GetFileText(string webRootPath)
        {
            var shellRobotsFile = System.IO.Path.Combine(webRootPath, "StaticRobots.txt");
            if (!System.IO.File.Exists(shellRobotsFile))
            {
                return string.Empty;
            }

            var shellRobotsText = await System.IO.File.ReadAllTextAsync(shellRobotsFile).ConfigureAwait(false);
            return !string.IsNullOrEmpty(shellRobotsText) ? shellRobotsText : string.Empty;
        }
    }
}