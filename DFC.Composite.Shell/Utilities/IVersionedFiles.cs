namespace DFC.Composite.Shell.Utilities
{
    public interface IVersionedFiles
    {
        string VersionedPathForAllIe8Css { get; }
        string VersionedPathForAllMinJs { get; }
        string VersionedPathForGovukMinCss { get; }
        string VersionedPathForJQueryBundleMinJs { get; }
        string VersionedPathForMainMinCss { get; }
        string VersionedPathForSiteCss { get; }
        string VersionedPathForSiteJs { get; }
    }
}