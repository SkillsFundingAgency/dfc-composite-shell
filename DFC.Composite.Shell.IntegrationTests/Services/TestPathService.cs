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

            paths.Add(new PathModel
            {
                Path = "path1",
                DocumentId = Guid.NewGuid(),
                IsOnline = true,
                Layout = PageLayout.FullWidth,
                TopNavigationOrder = 1,
                TopNavigationText = "Path1",
            });

            paths.Add(new PathModel
            {
                Path = "path2",
                DocumentId = Guid.NewGuid(),
                IsOnline = true,
                Layout = PageLayout.SidebarLeft,
                TopNavigationOrder = 2,
                TopNavigationText = "Path2",
            });

            paths.Add(new PathModel
            {
                Path = "path3",
                DocumentId = Guid.NewGuid(),
                IsOnline = false,
                Layout = PageLayout.SidebarRight,
                OfflineHtml = "Path3 is offline",
                TopNavigationOrder = 3,
                TopNavigationText = "Path3",
            });

            paths.Add(new PathModel
            {
                Path = "externalpath1",
                ExternalURL = "http://www.externalpath1.com",
                DocumentId = Guid.NewGuid(),
                IsOnline = true,
                Layout = PageLayout.None,
                TopNavigationOrder = 3,
            });

            return await Task.FromResult(paths).ConfigureAwait(false);
        }
    }
}