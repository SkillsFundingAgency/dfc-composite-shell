using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Services.Paths
{
    public class PathDataService : IPathDataService
    {
        private readonly IPathService _pathService;

        private IEnumerable<PathModel> _pathModels;

        public PathDataService(IPathService pathService)
        {
            _pathService = pathService;
        }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            if (_pathModels == null)
            {
                _pathModels = await _pathService.GetPaths();
            }

            return _pathModels;
        }

        public async Task<PathModel> GetPath(string path)
        {
            var pathModels = await GetPaths();

            return pathModels.FirstOrDefault(f => f.Path == path);
        }
    }
}
