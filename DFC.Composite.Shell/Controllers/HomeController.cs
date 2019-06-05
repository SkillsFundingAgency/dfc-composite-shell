using System.Diagnostics;
using DFC.Composite.Shell.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration configuration) : base(configuration) { }

        public IActionResult Index()
        {
            var pageViewModel = new PageViewModel()
            {
                PageTitle = "Home",
                BrandingAssetsCdn = _configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn))
            };

            return View(pageViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
