using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
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

            destination.LayoutName = $"{Constants.LayoutPrefix}{source?.Path.Layout.ToString()}";
            destination.Path = source?.Path.Path;
            destination.PageTitle = source?.Path.TopNavigationText;
            destination.PhaseBannerHtml = new HtmlString(source?.Path.PhaseBannerHtml);

            var pageRegionContentModels = source?.Regions
                .Select(region => new PageRegionContentModel { PageRegionType = region.PageRegion }).ToList();

            destination.PageRegionContentModels = pageRegionContentModels;
        }
    }
}