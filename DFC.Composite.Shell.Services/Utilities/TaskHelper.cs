using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Utilities
{
    [ExcludeFromCodeCoverage]
    public class TaskHelper : ITaskHelper
    {
        public bool TaskCompletedSuccessfully(Task task)
        {
            return task != null && task.IsCompletedSuccessfully;
        }
    }
}