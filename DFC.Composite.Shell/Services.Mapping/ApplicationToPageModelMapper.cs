using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.Mapping
{
    public class ApplicationToPageModelMapper : IMapper<ApplicationModel, PageViewModel>
    {
        public PageViewModel Map(ApplicationModel source)
        {
            var vm = new PageViewModel
            {
                LayoutName = $"{Constants.LayoutPrefix}{source.Path.Layout.ToString()}",
                Path = source.Path.Path
            };

            var pageRegionContentModels = new List<PageRegionContentModel>();

            foreach (var region in source.Regions)
            {
                var pageRegionContentModel = new PageRegionContentModel
                {
                    PageRegionType = region.PageRegion
                };

                pageRegionContentModels.Add(pageRegionContentModel);
            }
            vm.PageRegionContentModels = pageRegionContentModels;

            return vm;
        }
    }
}
