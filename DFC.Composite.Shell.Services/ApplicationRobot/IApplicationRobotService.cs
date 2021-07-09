using DFC.Composite.Shell.Models.Robots;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationRobot
{
    public interface IApplicationRobotService
    {
        Task<ApplicationRobotModel> EnrichAsync(ApplicationRobotModel model);
    }
}