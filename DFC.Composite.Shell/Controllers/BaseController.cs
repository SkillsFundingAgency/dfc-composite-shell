using System.Threading.Tasks;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IVersionedFiles _versionedFiles;

        public BaseController(IConfiguration configuration, IVersionedFiles versionedFiles)
        {
            _configuration = configuration;
            _versionedFiles = versionedFiles;
        }

        protected string BaseUrl()
        {
            return string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Url.Content("~"));
        }

        protected async Task<string> GetBearerTokenAsync()
        {
            return User.Identity.IsAuthenticated ? await HttpContext.GetTokenAsync(Common.Constants.BearerTokenName) : null;
        }

        protected PageViewModel BuildDefaultPageViewModel()
        {
            var viewModel = new PageViewModel
            {
                BrandingAssetsCdn = _configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn)),

                VersionedPathForMainMinCss = _versionedFiles.VersionedPathForMainMinCss,
                VersionedPathForGovukMinCss = _versionedFiles.VersionedPathForGovukMinCss,
                VersionedPathForAllIe8Css = _versionedFiles.VersionedPathForAllIe8Css,
                VersionedPathForSiteCss = _versionedFiles.VersionedPathForSiteCss,

                VersionedPathForJQueryBundleMinJs = _versionedFiles.VersionedPathForJQueryBundleMinJs,
                VersionedPathForAllMinJs = _versionedFiles.VersionedPathForAllMinJs,
                VersionedPathForSiteJs = _versionedFiles.VersionedPathForSiteJs
            };

            return viewModel;
        }
    }
}