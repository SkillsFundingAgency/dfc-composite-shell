using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace DFC.Composite.Shell.Views.Test.Extensions
{
    public static class IViewComponentResultExtensions
    {
        public static T ViewDataModelAs<T>(this IViewComponentResult viewComponentResult)
        {        
            var componentResult = viewComponentResult as ViewViewComponentResult;
            var viewComponentModel = (T)componentResult.ViewData.Model;

            return viewComponentModel;
        }
    }
}
