using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.Extensions.Configuration;
using System;

namespace DFC.Composite.Shell.Extensions
{
    public static class VersionedFilesExtensions
    {
        public static PageViewModel BuildDefaultPageViewModel(this IVersionedFiles versionedFiles, IConfiguration configuration)
        {
            _ = versionedFiles ?? throw new ArgumentNullException(nameof(versionedFiles));

            return new PageViewModel
            {
                BrandingAssetsCdn = configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn)),
                VersionedPathForMainMinCss = versionedFiles.VersionedPathForMainMinCss,
                VersionedPathForGovukMinCss = versionedFiles.VersionedPathForGovukMinCss,
                VersionedPathForAllIe8Css = versionedFiles.VersionedPathForAllIe8Css,
                VersionedPathForJQueryBundleMinJs = versionedFiles.VersionedPathForJQueryBundleMinJs,
                VersionedPathForAllMinJs = versionedFiles.VersionedPathForAllMinJs,
                VersionedPathForDfcDigitalMinJs = versionedFiles.VersionedPathForDfcDigitalMinJs,
                VersionedPathForCompUiMinJs = versionedFiles.VersionedPathForCompUiMinJs,
                VersionedPathForWebChatJs = versionedFiles.VersionedPathForWebChatJs,
                WebchatEnabled = versionedFiles.WebchatEnabled,
            };
        }
    }
}