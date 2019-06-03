using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _configuration;

        public BaseController( IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}