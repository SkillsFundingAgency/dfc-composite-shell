using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;

namespace DFC.Composite.Shell.Services.DataProtectionProviders
{
    public class CompositeDataProtectionDataProvider : ICompositeDataProtectionDataProvider
    {
        private readonly IDataProtector dataProtector;
        private readonly ILogger<CompositeDataProtectionDataProvider> logger;

        public CompositeDataProtectionDataProvider(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<CompositeDataProtectionDataProvider> logger)
        {
            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            dataProtector = dataProtectionProvider.CreateProtector(nameof(CompositeDataProtectionDataProvider));
            this.logger = logger;
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
            catch (CryptographicException ex)
            {
                logger.LogWarning("{dataProvider} Unprotect. Error occured {ex}", nameof(CompositeDataProtectionDataProvider), ex);
            }

            return result;
        }
    }
}
