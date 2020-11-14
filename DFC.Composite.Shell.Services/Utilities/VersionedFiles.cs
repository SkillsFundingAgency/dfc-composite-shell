using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.AppRegistry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DFC.Composite.Shell.Utilities
{
    public class VersionedFiles : IVersionedFiles
    {
        public VersionedFiles(WebchatOptions webchatOptions, IAppRegistryDataService appRegistryDataService)
        {
            _ = webchatOptions ?? throw new ArgumentNullException(nameof(webchatOptions));
            _ = appRegistryDataService ?? throw new ArgumentNullException(nameof(appRegistryDataService));

            var shellAppRegistrationModel = appRegistryDataService.GetShellAppRegistrationModel().Result;

            if (shellAppRegistrationModel.CssScriptNames != null && shellAppRegistrationModel.CssScriptNames.Any())
            {
                foreach (var key in shellAppRegistrationModel.CssScriptNames.Keys)
                {
                    var value = shellAppRegistrationModel.CssScriptNames[key];
                    var fullPathname = key.StartsWith("/", StringComparison.Ordinal) ? shellAppRegistrationModel.CdnLocation + key : key;

                    VersionedPathForCssScripts.Add($"{fullPathname}?{value}");
                }
            }

            if (shellAppRegistrationModel.JavaScriptNames != null && shellAppRegistrationModel.JavaScriptNames.Any())
            {
                foreach (var key in shellAppRegistrationModel.JavaScriptNames.Keys)
                {
                    var value = shellAppRegistrationModel.JavaScriptNames[key];

                    if (key.Equals(webchatOptions.ScriptUrl, StringComparison.OrdinalIgnoreCase))
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

        public IList<string> VersionedPathForCssScripts { get; } = new List<string>();

        public IList<string> VersionedPathForJavaScripts { get; } = new List<string>();

        public string VersionedPathForWebChatJs { get; }

        public bool WebchatEnabled { get; }
    }
}
