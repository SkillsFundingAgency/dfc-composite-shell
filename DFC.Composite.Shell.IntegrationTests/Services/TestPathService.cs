using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Integration.Test.Services
{
    public class TestPathService : IAppRegistryService
    {
        public async Task<IEnumerable<AppRegistrationModel>> GetPaths()
        {
            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = "path1",
                    IsOnline = true,
                    Layout = PageLayout.FullWidth,
                    TopNavigationOrder = 1,
                    TopNavigationText = "Path1",
                },

                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = "path4",
                    IsOnline = true,
                    Layout = PageLayout.FullWidthNoMain,
                    TopNavigationOrder = 1,
                    TopNavigationText = "Path4",
                },

                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = "path2",
                    IsOnline = true,
                    Layout = PageLayout.SidebarLeft,
                    TopNavigationOrder = 2,
                    TopNavigationText = "Path2",
                },

                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = "path3",
                    IsOnline = false,
                    Layout = PageLayout.SidebarRight,
                    OfflineHtml = "Path3 is offline",
                    TopNavigationOrder = 3,
                    TopNavigationText = "Path3",
                },

                new AppRegistrationModel
                {
                    Id = Guid.NewGuid(),
                    Path = "externalpath1",
                    ExternalURL = new Uri("http://www.externalpath1.com", UriKind.Absolute),
                    IsOnline = true,
                    Layout = PageLayout.None,
                    TopNavigationOrder = 3,
                },
            };

            return await Task.FromResult(appRegistrationModels).ConfigureAwait(false);
        }

        public async Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}