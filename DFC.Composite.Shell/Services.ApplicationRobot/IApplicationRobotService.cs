using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationRobot
{
    public interface IApplicationRobotService
    {
        string Path { get; set; }

        string BearerToken { get; set; }

        string RobotsURL { get; set; }

        Task<string> TheTask { get; set; }

        Task<string> GetAsync();
    }
}