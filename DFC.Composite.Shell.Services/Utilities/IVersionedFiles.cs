using System.Collections.Generic;

namespace DFC.Composite.Shell.Utilities
{
    public interface IVersionedFiles
    {
        string VersionedPathForAllIe8Css { get; }

        string VersionedPathForGovukMinCss { get; }

        string VersionedPathForMainMinCss { get; }

        IList<string> VersionedPathForJavaScripts { get; }

        string VersionedPathForWebChatJs { get; }

        public bool WebchatEnabled { get; }
    }
}