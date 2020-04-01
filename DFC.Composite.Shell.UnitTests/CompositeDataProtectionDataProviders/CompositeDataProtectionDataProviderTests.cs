using DFC.Composite.Shell.Services.DataProtectionProviders;
using FakeItEasy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.CompositeDataProtectionDataProviders
{
    public class CompositeDataProtectionDataProviderTests
    {
        private readonly CompositeDataProtectionDataProvider compositeDataProtectionDataProvider;
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly ILogger<CompositeDataProtectionDataProvider> logger;

        public CompositeDataProtectionDataProviderTests()
        {
            dataProtectionProvider = A.Fake<IDataProtectionProvider>();
            logger = A.Fake<ILogger<CompositeDataProtectionDataProvider>>();
            compositeDataProtectionDataProvider = new CompositeDataProtectionDataProvider(dataProtectionProvider, logger);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("abc")]
        public void UnprotectingThrowsExceptionIfPayloadInNotValid(string protectedValue)
        {
            A.CallTo(() => dataProtectionProvider.CreateProtector(protectedValue)).Throws<CryptographicException>();

            Assert.Equal(string.Empty, compositeDataProtectionDataProvider.Unprotect(protectedValue));
        }
    }
}
