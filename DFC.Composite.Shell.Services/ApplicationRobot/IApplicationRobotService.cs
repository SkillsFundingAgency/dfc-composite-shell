using DFC.Composite.Shell.Models.Robots;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationRobot
{
    public interface IApplicationRobotService
    {
        Task<string> GetAsync(ApplicationRobotModel model);
    }
}