using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Paths
{
    public class PathDataService : IPathDataService
    {
        private readonly IPathService pathService;
        private IEnumerable<PathModel> pathModels;

        public PathDataService(IPathService pathService)
        {
            this.pathService = pathService;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            return pathModels ?? (pathModels = await pathService.GetPaths().ConfigureAwait(false));
        }

        public async Task<PathModel> GetPath(string path)
        {
            var paths = await GetPaths().ConfigureAwait(false);

            return paths.FirstOrDefault(f => f.Path.ToLower(CultureInfo.CurrentCulture) == path.ToLower(CultureInfo.CurrentCulture));
        }
    }
}