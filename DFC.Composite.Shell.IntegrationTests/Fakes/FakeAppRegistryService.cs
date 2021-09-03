using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.IntegrationTests.Fakes
{
    public class FakeAppRegistryService : IAppRegistryService
    {
        public async Task<IEnumerable<AppRegistrationModel>> GetPaths()
        {
            var appRegistrationModels = new List<AppRegistrationModel>
            {
                new AppRegistrationModel
                {
                    Path = "path1",
                    IsOnline = true,
                    Layout = PageLayout.FullWidth,
                    TopNavigationOrder = 1,
                    TopNavigationText = "Path1",
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Head,
                            RegionEndpoint = "http://www.expected-domain.com/expected-path/{0}/head",
                        },
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            RegionEndpoint = "http://www.expected-domain.com/expected-path/{0}/body",
                        },
                    },
                },
                new AppRegistrationModel
                {
                    Path = "path4",
                    IsOnline = true,
                    Layout = PageLayout.FullWidthNoMain,
                    TopNavigationOrder = 1,
                    TopNavigationText = "Path4",
                },
                new AppRegistrationModel
                {
                    Path = "path2",
                    IsOnline = true,
                    Layout = PageLayout.SidebarLeft,
                    TopNavigationOrder = 2,
                    TopNavigationText = "Path2",
                },
                new AppRegistrationModel
                {
                    Path = "path3",
                    IsOnline = false,
                    Layout = PageLayout.SidebarRight,
                    OfflineHtml = "Path3 is offline",
                    TopNavigationOrder = 3,
                    TopNavigationText = "Path3",
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            RegionEndpoint = "http://www.expected-domain.com/expected-path/{0}/body",
                        },
                    },
                },
                new AppRegistrationModel
                {
                    Path = "externalpath1",
                    ExternalURL = new Uri("http://www.externalpath1.com", UriKind.Absolute),
                    IsOnline = true,
                    Layout = PageLayout.None,
                    TopNavigationOrder = 3,
                },
                new AppRegistrationModel
                {
                    Path = "pages",
                    IsOnline = true,
                    Layout = PageLayout.None,
                    TopNavigationOrder = 3,
                    PageLocations = new Dictionary<Guid, PageLocationModel>
                    {
                        {
                            Guid.NewGuid(),
                            new PageLocationModel
                            {
                                Locations = new List<string>
                                {
                                    "/path1",
                                },
                            }
                        },
                    },
                    Regions = new List<RegionModel>
                    {
                        new RegionModel
                        {
                            PageRegion = PageRegion.Body,
                            RegionEndpoint = "http://www.expected-domain.com/expected-path/{0}/body",
                        },
                    },
                },
            };

            return await Task.FromResult(appRegistrationModels);
        }

        public Task<bool> SetRegionHealthState(string path, PageRegion pageRegion, bool isHealthy)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SetAjaxRequestHealthState(string path, string name, bool isHealthy)
        {
            return Task.FromResult(true);
        }
    }
}
