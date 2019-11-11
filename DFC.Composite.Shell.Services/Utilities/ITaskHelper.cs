using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Utilities
{
    public interface ITaskHelper
    {
        bool TaskCompletedSuccessfully(Task task);
    }
}