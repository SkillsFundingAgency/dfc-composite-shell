using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DFC.Composite.Shell.Utilities
{
    public class VersionedFiles : IVersionedFiles
    {
        public VersionedFiles(IConfiguration configuration, IAssetLocationAndVersionService assetLocationAndVersionService, WebchatOptions webchatOptions, IAppRegistryDataService appRegistryDataService)
        {
            _ = webchatOptions ?? throw new ArgumentNullException(nameof(webchatOptions));
            _ = appRegistryDataService ?? throw new ArgumentNullException(nameof(appRegistryDataService));

            var brandingAssetsCdn = configuration.GetValue<string>("BrandingAssetsCdn");
            var brandingAssetsFolder = $"{brandingAssetsCdn}/{Constants.NationalCareersToolkit}";

            VersionedPathForMainMinCss = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/all.min.css");
            VersionedPathForGovukMinCss = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/govuk.min.css");
            VersionedPathForAllIe8Css = assetLocationAndVersionService?.GetCdnAssetFileAndVersion($"{brandingAssetsFolder}/css/all-ie8.css");

            var shellAppRegistrationModel = appRegistryDataService.GetShellAppRegistrationModel().Result;

            if (shellAppRegistrationModel.JavaScriptNames != null && shellAppRegistrationModel.JavaScriptNames.Any())
            {
                foreach (var key in shellAppRegistrationModel.JavaScriptNames.Keys)
                {
                    var value = shellAppRegistrationModel.JavaScriptNames[key];

                    if (key.EndsWith("/js/chatRed.js", StringComparison.OrdinalIgnoreCase))
                    {
                        VersionedPathForWebChatJs = $"{key}?{value}";
                    }
                    else
                    {
                        var fullPathname = key.StartsWith("/", StringComparison.Ordinal) ? shellAppRegistrationModel.CdnLocation + key : key;

                        VersionedPathForJavaScripts.Add($"{fullPathname}?{value}");
                    }
                }
            }

            if (webchatOptions.Enabled)
            {
                WebchatEnabled = webchatOptions.Enabled;
            }
        }

        public string VersionedPathForMainMinCss { get; }

        public string VersionedPathForGovukMinCss { get; }

        public string VersionedPathForAllIe8Css { get; }

        public IList<string> VersionedPathForJavaScripts { get; } = new List<string>();

        public string VersionedPathForWebChatJs { get; }

        public bool WebchatEnabled { get; }
    }
}
