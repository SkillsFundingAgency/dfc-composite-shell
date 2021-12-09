using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ShellRobotFile
{
    public interface IShellRobotFileService
    {
        Task<string> GetStaticFileText(string webRootPath);
    }
}