using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Paths;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class TestPathService : IPathService
    {
        public async Task<IEnumerable<PathModel>> GetPaths()
        {
            var paths = new List<PathModel>();

            paths.Add(new PathModel()
            {
                Path = "path1",
                DocumentId = Guid.NewGuid(),
                IsOnline = true,
                Layout = Common.PageLayout.FullWidth,
                TopNavigationOrder = 1,
                TopNavigationText = "Path1"
            });

            paths.Add(new PathModel()
            {
                Path = "path2",
                DocumentId = Guid.NewGuid(),
                IsOnline = true,
                Layout = Common.PageLayout.SidebarLeft,
                TopNavigationOrder = 2,
                TopNavigationText = "Path2"
            });

            paths.Add(new PathModel()
            {
                Path = "externalpath1",
                ExternalURL = "http://www.externalpath1.com",
                DocumentId = Guid.NewGuid(),
                IsOnline = true,
                Layout = Common.PageLayout.None,
                TopNavigationOrder = 3
            });

            return await Task.FromResult(paths);
        }
    }
}
