using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Paths
{
    public interface IPathService
    {
        Task<IEnumerable<PathModel>> GetPaths();
        Task<PathModel> GetPath(string path);
    }
}
