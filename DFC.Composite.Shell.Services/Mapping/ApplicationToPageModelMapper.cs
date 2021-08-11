using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.AppRegistry;

using Microsoft.AspNetCore.Html;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Mapping
{
    public class ApplicationToPageModelMapper : IMapper<ApplicationModel, PageViewModel>
    {
        private readonly IAppRegistryDataService appRegistryDataService;

        public ApplicationToPageModelMapper(IAppRegistryDataService appRegistryDataService)
        {
            this.appRegistryDataService = appRegistryDataService;
        }

        public async Task Map(ApplicationModel source, PageViewModel destination)
        {
            if (destination == null)
            {
                return;
            }

            destination.LayoutName = $"{Constants.LayoutPrefix}{source?.AppRegistrationModel.Layout.ToString()}";
            destination.Path = source?.AppRegistrationModel.Path;
            destination.PageTitle = source?.AppRegistrationModel.TopNavigationText;
            destination.PhaseBannerHtml = new HtmlString(source?.AppRegistrationModel.PhaseBannerHtml);

            var pageRegionContentModels = source?.AppRegistrationModel?.Regions?
                .Select(region => new PageRegionContentModel { PageRegionType = region.PageRegion }).ToList();

            destination.PageRegionContentModels = pageRegionContentModels;

            var shellAppRegistrationModel = await appRegistryDataService.GetShellAppRegistrationModel();

            if (shellAppRegistrationModel != null)
            {
                if (source?.AppRegistrationModel?.CssScriptNames != null && source.AppRegistrationModel.CssScriptNames.Any())
                {
                    foreach (var key in source.AppRegistrationModel.CssScriptNames.Keys)
                    {
                        var value = source.AppRegistrationModel.CssScriptNames[key];

                        var fullPathname = key.StartsWith("/", StringComparison.Ordinal) ? shellAppRegistrationModel.CdnLocation + key : key;

                        destination.VersionedPathForCssScripts.Add($"{fullPathname}?{value}");
                    }
                }
                if (source?.AppRegistrationModel?.JavaScriptNames != null && source.AppRegistrationModel.JavaScriptNames.Any())
                {
                    foreach (var key in source.AppRegistrationModel.JavaScriptNames.Keys)
                    {
                        var value = source.AppRegistrationModel.JavaScriptNames[key];

                        var fullPathname = key.StartsWith("/", StringComparison.Ordinal) ? shellAppRegistrationModel.CdnLocation + key : key;

                        destination.VersionedPathForJavaScripts.Add($"{fullPathname}?{value}");
                    }
                }
            }
        }
    }
}