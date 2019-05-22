using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Paths
{
    public class LocalPathService : IPathService
    {
        public PathModel Path { get; set; }

        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var result = new List<PathModel>();

            var pathModel1 = new PathModel();
            pathModel1.Layout = Layout.FullWidth;
            pathModel1.Path = "child1";
            pathModel1.TopNavigationOrder = 100;
            pathModel1.TopNavigationText = "Child1";
            pathModel1.OfflineHtml = "<strong>Child1 is Offline</strong>";
            result.Add(pathModel1);

            return await Task.FromResult(result);
        }
    }
}
