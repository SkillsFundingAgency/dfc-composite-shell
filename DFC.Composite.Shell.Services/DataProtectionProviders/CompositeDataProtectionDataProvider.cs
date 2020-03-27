using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography;

namespace DFC.Composite.Shell.Services.DataProtectionProviders
{
    public class CompositeDataProtectionDataProvider : ICompositeDataProtectionDataProvider
    {
        private readonly IDataProtector dataProtector;

        public CompositeDataProtectionDataProvider(IDataProtectionProvider dataProtectionProvider)
        {
            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            this.dataProtector = dataProtectionProvider.CreateProtector(nameof(CompositeDataProtectionDataProvider));
        }

        public string Protect(string value)
        {
            return dataProtector.Protect(value);
        }

        public string Unprotect(string value)
        {
            var result = string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    result = dataProtector.Unprotect(value);
                }
            }
            catch (CryptographicException)
            {
            }

            return result;
        }
    }
}
