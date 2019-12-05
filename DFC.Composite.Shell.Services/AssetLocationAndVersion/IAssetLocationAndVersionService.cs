namespace DFC.Composite.Shell.Services.AssetLocationAndVersion
{
    public interface IAssetLocationAndVersionService
    {
        string GetCdnAssetFileAndVersion(string assetLocation);

        string GetLocalAssetFileAndVersion(string assetLocation);
    }
}