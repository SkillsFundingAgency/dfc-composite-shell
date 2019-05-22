using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            var pageViewModel = new Models.PageViewModel()
            {
                PageTitle = "Home"
            };

            return View(pageViewModel);
        }

        public IActionResult Privacy()
        {
            var pageViewModel = new Models.PageViewModel()
            {
                PageTitle = "Privacy Policy"
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
