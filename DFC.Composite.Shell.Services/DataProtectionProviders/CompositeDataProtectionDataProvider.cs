using Microsoft.AspNetCore.DataProtection;

namespace DFC.Composite.Shell.Services.DataProtectionProviders
{
    public class CompositeDataProtectionDataProvider : ICompositeDataProtectionDataProvider
    {
        private readonly IDataProtector dataProtector;

        public CompositeDataProtectionDataProvider(IDataProtectionProvider dataProtectionProvider)
        {
            this.dataProtector = dataProtectionProvider.CreateProtector(nameof(CompositeDataProtectionDataProvider));
        }

        public string Protect(string value)
        {
            return dataProtector.Protect(value);
        }

        public string Unprotect(string value)
        {
            return dataProtector.Unprotect(value);
        }
    }
}
