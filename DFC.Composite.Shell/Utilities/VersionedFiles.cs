using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Utilities
{
    public class VersionedFiles : IVersionedFiles
    {
        protected readonly IConfiguration _configuration;
        protected readonly IAssetLocationAndVersion _assetLocationAndVersion;

        public string VersionedPathForMainMinCss { get; }
        public string VersionedPathForGovukMinCss { get; }
        public string VersionedPathForAllIe8Css { get; }
        public string VersionedPathForSiteCss { get; }

        public string VersionedPathForJQueryBundleMinJs { get; }
        public string VersionedPathForAllMinJs { get; }
        public string VersionedPathForSiteJs { get; }

        public VersionedFiles(IConfiguration configuration, IAssetLocationAndVersion assetLocationAndVersion)
        {
            _configuration = configuration;
            _assetLocationAndVersion = assetLocationAndVersion;

            string brandingAssetsCdn = _configuration.GetValue<string>("BrandingAssetsCdn");
            string brandingAssetsFolder = $"{brandingAssetsCdn}/gds_service_toolkit";

            VersionedPathForMainMinCss = _assetLocationAndVersion.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/main.min.css");
            VersionedPathForGovukMinCss = _assetLocationAndVersion.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/govuk.min.css");
            VersionedPathForAllIe8Css = _assetLocationAndVersion.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/all-ie8.css");
            VersionedPathForSiteCss = _assetLocationAndVersion.GetLocalAssetFileAndVersion("css/site.css");

            VersionedPathForJQueryBundleMinJs = _assetLocationAndVersion.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/js/jquerybundle.min.js");
            VersionedPathForAllMinJs = _assetLocationAndVersion.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/js/all.min.js");
            VersionedPathForSiteJs = _assetLocationAndVersion.GetLocalAssetFileAndVersion("js/site.js");
        }
    }
}
