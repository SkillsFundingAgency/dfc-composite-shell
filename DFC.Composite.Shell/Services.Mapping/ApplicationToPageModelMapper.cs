using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.Mapping
{
    public class ApplicationToPageModelMapper : IMapper<ApplicationModel, PageViewModel>
    {
        public void Map(ApplicationModel source, PageViewModel destination)
        {
            destination.LayoutName = $"{Constants.LayoutPrefix}{source.Path.Layout.ToString()}";
            destination.Path = source.Path.Path;
            destination.PageTitle = source.Path.TopNavigationText;
            destination.PhaseBannerHtml = source.Path.PhaseBannerHtml;

            var pageRegionContentModels = new List<PageRegionContentModel>();

            foreach (var region in source.Regions)
            {
                var pageRegionContentModel = new PageRegionContentModel
                {
                    PageRegionType = region.PageRegion
                };

                pageRegionContentModels.Add(pageRegionContentModel);
            }
            destination.PageRegionContentModels = pageRegionContentModels;
        }
    }
}
