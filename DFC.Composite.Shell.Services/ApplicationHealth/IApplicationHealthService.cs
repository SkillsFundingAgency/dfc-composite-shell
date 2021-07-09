using DFC.Composite.Shell.Models.Health;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ApplicationHealth
{
    public interface IApplicationHealthService
    {
        Task<ApplicationHealthModel> EnrichAsync(ApplicationHealthModel model);
    }
}
