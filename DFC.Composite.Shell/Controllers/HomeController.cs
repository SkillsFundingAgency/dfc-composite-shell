using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace DFC.Composite.Shell.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration configuration, IVersionedFiles versionedFiles) : base(configuration, versionedFiles)
        {
        }

        public IActionResult Index()
        {
            var viewModel = BuildDefaultPageViewModel();

            viewModel.PageTitle = "Home";

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}