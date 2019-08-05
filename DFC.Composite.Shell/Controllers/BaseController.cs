using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class BaseController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IVersionedFiles versionedFiles;

        public BaseController(IConfiguration configuration, IVersionedFiles versionedFiles)
        {
            this.configuration = configuration;
            this.versionedFiles = versionedFiles;
        }

        protected string BaseUrl()
        {
            return $"{Request.Scheme}://{Request.Host}{Url.Content("~")}";
        }

        protected async Task<string> GetBearerTokenAsync()
        {
            return User.Identity.IsAuthenticated ? await HttpContext.GetTokenAsync(Common.Constants.BearerTokenName).ConfigureAwait(false) : null;
        }

        protected PageViewModel BuildDefaultPageViewModel()
        {
            return new PageViewModel
            {
                BrandingAssetsCdn = configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn)),
                VersionedPathForMainMinCss = versionedFiles.VersionedPathForMainMinCss,
                VersionedPathForGovukMinCss = versionedFiles.VersionedPathForGovukMinCss,
                VersionedPathForAllIe8Css = versionedFiles.VersionedPathForAllIe8Css,
                VersionedPathForSiteCss = versionedFiles.VersionedPathForSiteCss,
                VersionedPathForJQueryBundleMinJs = versionedFiles.VersionedPathForJQueryBundleMinJs,
                VersionedPathForAllMinJs = versionedFiles.VersionedPathForAllMinJs,
                VersionedPathForSiteJs = versionedFiles.VersionedPathForSiteJs,
            };
        }
    }
}