namespace DFC.Composite.Shell.Services.AssetLocationAndVersion
{
    public interface IAssetLocationAndVersion
    {
        string GetCdnAssetFileAndVersion(string assetLocation);
        string GetLocalAssetFileAndVersion(string assetLocation);
    }
}