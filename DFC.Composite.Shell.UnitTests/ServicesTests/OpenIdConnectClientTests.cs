using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class OpenIdConnectClientTests
    {
        private const string DefaultSignInRedirectUrl = "testSignInRedirect.com";
        private const string DefaultSignOutRedirectUrl = "testSignOutRedirect.com";
        private readonly IOptions<OpenIDConnectSettings> defaultSettings;
        private readonly SecurityTokenHandler tokenHandler;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;

        public OpenIdConnectClientTests()
        {
            defaultSettings = Options.Create(new OpenIDConnectSettings
            {
                RedirectUrl = DefaultSignInRedirectUrl,
                SignOutRedirectUrl = DefaultSignOutRedirectUrl,
                Issuer = "issuer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
                Exponent = "AQAB",
            });

            tokenHandler = A.Fake<SecurityTokenHandler>();
            configurationManager = A.Fake<IConfigurationManager<OpenIdConnectConfiguration>>();
            A.CallTo(() => configurationManager.GetConfigurationAsync(CancellationToken.None)).Returns(
                new OpenIdConnectConfiguration
                {
                    AuthorizationEndpoint = "auth",
                    EndSessionEndpoint = "end",
                    Issuer = "issuer",
                });
        }

        [Fact]
        public async Task WhenGetSignInUrlCalledWithoutParameterThenReturnUrlWithDefaultRedirect()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, configurationManager);

            var url = await client.GetSignInUrl().ConfigureAwait(false);

            Assert.Contains(DefaultSignInRedirectUrl, url, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task WhenGetSignOutUrlCalledWithParameterThenReturnUrlWithSuppliedRedirect()
        {
            var redirect = "RedirectFromChild";
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, configurationManager);

            var url = await client.GetSignOutUrl(redirect).ConfigureAwait(false);

            Assert.Contains(redirect, url, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task WhenGetSignOutUrlCalledWithoutParameterThenReturnUrlWithDefaultRedirect()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, configurationManager);

            var url = await client.GetSignOutUrl(string.Empty).ConfigureAwait(false);

            Assert.Contains(DefaultSignOutRedirectUrl, url, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task WhenGetRegisterUrlCalledWithoutParameterThenReturnUrlWithDefaultRedirect()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, configurationManager);

            var url = await client.GetRegisterUrl().ConfigureAwait(false);

            Assert.Contains(DefaultSignInRedirectUrl, url, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task WhenValidateTokenCalledThenAttemptToValidateToken()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, configurationManager);
            SecurityToken secToken;
            var token = await client.ValidateToken("token").ConfigureAwait(true);
            A.CallTo(() => tokenHandler.ValidateToken(A<string>.Ignored, A<TokenValidationParameters>.That.Matches(x => x.ValidIssuer == defaultSettings.Value.Issuer), out secToken)).MustHaveHappened();
        }

        [Fact]
        public async Task
            WhenValidateTokenCalledAndUseOidConfigDiscoveryThenAttemptToValidateTokenUsingDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = DefaultSignInRedirectUrl,
                SignOutRedirectUrl = DefaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
                Exponent = "AQAB",
            });

            var client = new AzureB2CAuthClient(settings, tokenHandler, configurationManager);

            SecurityToken secToken;
            var token = await client.ValidateToken("token").ConfigureAwait(true);
            A.CallTo(() => tokenHandler.ValidateToken(A<string>.Ignored, A<TokenValidationParameters>.That.Matches(x => x.ValidIssuer == "issuer"), out secToken))
                .MustHaveHappened();
        }

        [Fact]
        public async Task WhenGetSignOutCalledAndUseOidConfigDiscoveryThenUseDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = DefaultSignInRedirectUrl,
                SignOutRedirectUrl = DefaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var client = new AzureB2CAuthClient(settings, tokenHandler, configurationManager);

            var token = await client.GetSignOutUrl("test").ConfigureAwait(true);
            A.CallTo(() => configurationManager.GetConfigurationAsync(CancellationToken.None)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenGetSignInCalledAndUseOidConfigDiscoveryThenUseDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = DefaultSignInRedirectUrl,
                SignOutRedirectUrl = DefaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var client = new AzureB2CAuthClient(settings, tokenHandler, configurationManager);

            var token = await client.GetSignInUrl().ConfigureAwait(true);
            A.CallTo(() => configurationManager.GetConfigurationAsync(CancellationToken.None)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenGetRegisterUrlCalledAndUseOidConfigDiscoveryThenUseDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = DefaultSignInRedirectUrl,
                SignOutRedirectUrl = DefaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var client = new AzureB2CAuthClient(settings, tokenHandler, configurationManager);

            var token = await client.GetRegisterUrl().ConfigureAwait(true);
            A.CallTo(() => configurationManager.GetConfigurationAsync(CancellationToken.None)).MustHaveHappened();
        }
    }
}
