using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;

namespace DFC.Composite.Shell.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVersionedFiles versionedFiles;
        private readonly IConfiguration configuration;

        public HomeController(IVersionedFiles versionedFiles, IConfiguration configuration)
        {
            this.versionedFiles = versionedFiles;
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            viewModel.PageTitle = "Home";

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/home/alert/{statusCode}")]
        public IActionResult Alert(HttpStatusCode statusCode)
        {
            string viewName = nameof(Alert);
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            viewModel.PageTitle = "Error | Explore careers | National Careers Service";

            switch (statusCode)
            {
                case HttpStatusCode.NotFound:
                    viewName += $"{(int)statusCode}";
                    viewModel.PageTitle = "Page not found";
                    break;
            }

            return View(viewName, viewModel);
        }
    }
}