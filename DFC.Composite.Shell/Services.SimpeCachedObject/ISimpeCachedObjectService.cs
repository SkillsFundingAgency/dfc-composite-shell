namespace DFC.Composite.Shell.Services.SimpeCachedObject
{
    public interface ISimpeCachedObjectService<T>
    {
        T CachedObject { get; set; }
        int CacheDurationInSeconds { get; set; }
    }
}