using Microsoft.AspNetCore.Mvc;

namespace DFC.Composite.Shell.ViewComponents
{
    public class ShowHelpLinksViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}