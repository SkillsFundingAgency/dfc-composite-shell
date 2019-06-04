using System.Threading.Tasks;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Mapping;
using Microsoft.AspNetCore.Authentication;
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

        protected string BaseUrl()
        {
            return string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Url.Content("~"));
        }

        protected async Task<string> GetBearerTokenAsync()
        {
            return User.Identity.IsAuthenticated ? await HttpContext.GetTokenAsync(Common.Constants.BearerTokenName) : null;
        }
    }
}