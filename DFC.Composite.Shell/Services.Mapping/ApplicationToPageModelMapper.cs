using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.Mapping
{
    public class ApplicationToPageModelMapper : IMapper<ApplicationModel, PageModel>
    {
        public PageModel Map(ApplicationModel source)
        {
            var vm = new PageModel();
            vm.LayoutName = $"{Constants.LayoutPrefix}{source.Path.Layout.ToString()}";
            vm.Path = source.Path.Path;

            var pageRegionContentModels = new List<PageRegionContentModel>();
            foreach (var region in source.Regions)
            {
                var pageRegionContentModel = new PageRegionContentModel();
                pageRegionContentModel.PageRegionType = region.PageRegion;
                pageRegionContentModels.Add(pageRegionContentModel);
            }
            vm.PageRegionContentModels = pageRegionContentModels;

            return vm;
        }
    }
}
