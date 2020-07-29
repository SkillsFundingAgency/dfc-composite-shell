using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Common;
using Microsoft.AspNetCore.Html;
using System.Linq;

namespace DFC.Composite.Shell.Services.Mapping
{
    public class ApplicationToPageModelMapper : IMapper<ApplicationModel, PageViewModel>
    {
        public void Map(ApplicationModel source, PageViewModel destination)
        {
            if (destination == null)
            {
                return;
            }

            destination.LayoutName = $"{Constants.LayoutPrefix}{source?.AppRegistrationModel.Layout.ToString()}";
            destination.Path = source?.AppRegistrationModel.Path;
            destination.PageTitle = source?.AppRegistrationModel.TopNavigationText;
            destination.PhaseBannerHtml = new HtmlString(source?.AppRegistrationModel.PhaseBannerHtml);

            var pageRegionContentModels = source?.AppRegistrationModel?.Regions
                .Select(region => new PageRegionContentModel { PageRegionType = region.PageRegion }).ToList();

            destination.PageRegionContentModels = pageRegionContentModels;
        }
    }
}