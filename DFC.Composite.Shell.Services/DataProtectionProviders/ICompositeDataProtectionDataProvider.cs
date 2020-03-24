namespace DFC.Composite.Shell.Services.DataProtectionProviders
{
    public interface ICompositeDataProtectionDataProvider
    {
        string Protect(string value);

        string Unprotect(string value);
    }
}
