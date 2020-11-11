using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

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
                VersionedPathForCssScripts = versionedFiles.VersionedPathForCssScripts.ToList(),
                VersionedPathForJavaScripts = versionedFiles.VersionedPathForJavaScripts.ToList(),
                VersionedPathForWebChatJs = versionedFiles.VersionedPathForWebChatJs,
                WebchatEnabled = versionedFiles.WebchatEnabled,
            };
        }
    }
}