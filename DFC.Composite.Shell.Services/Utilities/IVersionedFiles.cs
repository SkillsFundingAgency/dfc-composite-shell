using System.Collections.Generic;

namespace DFC.Composite.Shell.Utilities
{
    public interface IVersionedFiles
    {
        IList<string> VersionedPathForCssScripts { get; }

        IList<string> VersionedPathForJavaScripts { get; }

        string VersionedPathForWebChatJs { get; }

        public bool WebchatEnabled { get; }
    }
}