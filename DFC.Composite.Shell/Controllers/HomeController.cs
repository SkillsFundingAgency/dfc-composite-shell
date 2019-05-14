using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DFC.Composite.Shell.Models;

namespace DFC.Composite.Shell.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var pageViewModel = new Models.PageViewModel()
            {
                PageTitle = "Home",
                Branding = "ESFA"
            };

            return View(pageViewModel);
        }

        public IActionResult Privacy()
        {
            var pageViewModel = new Models.PageViewModel()
            {
                PageTitle = "Privacy Policy",
                Branding = "ESFA"
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
