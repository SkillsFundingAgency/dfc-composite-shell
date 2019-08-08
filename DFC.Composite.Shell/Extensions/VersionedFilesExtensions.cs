using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Extensions
{
    public static class VersionedFilesExtensions
    {
        public static PageViewModel BuildDefaultPageViewModel(this IVersionedFiles versionedFiles, IConfiguration configuration)
        {
            return new PageViewModel
            {
                BrandingAssetsCdn = configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn)),
                VersionedPathForMainMinCss = versionedFiles?.VersionedPathForMainMinCss,
                VersionedPathForGovukMinCss = versionedFiles?.VersionedPathForGovukMinCss,
                VersionedPathForAllIe8Css = versionedFiles?.VersionedPathForAllIe8Css,
                VersionedPathForSiteCss = versionedFiles?.VersionedPathForSiteCss,
                VersionedPathForJQueryBundleMinJs = versionedFiles?.VersionedPathForJQueryBundleMinJs,
                VersionedPathForAllMinJs = versionedFiles?.VersionedPathForAllMinJs,
                VersionedPathForSiteJs = versionedFiles?.VersionedPathForSiteJs,
            };
        }
    }
}