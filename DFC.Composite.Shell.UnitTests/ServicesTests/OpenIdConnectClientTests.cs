using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class OpenIdConnectClientTests
    {
        private readonly IOptions<OpenIDConnectSettings> defaultSettings;
        private readonly SecurityTokenHandler tokenHandler;
        const string defaultSignInRedirectUrl = "testSignInRedirect.com";
        const string defaultSignOutRedirectUrl = "testSignOutRedirect.com";
        private readonly IOpenIdConnectService openIdConnectService;

        public OpenIdConnectClientTests()
        {
            defaultSettings = Options.Create(new OpenIDConnectSettings
            {
                RedirectUrl = defaultSignInRedirectUrl,
                SignOutRedirectUrl = defaultSignOutRedirectUrl,
                Issuer = "issuer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            tokenHandler = A.Fake<SecurityTokenHandler>();
            openIdConnectService = A.Fake<IOpenIdConnectService>();
        }

        [Fact]
        public async Task WhenGetSignInUrlCalledWithoutParameterThenReturnUrlWithDefaultRedirect()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, openIdConnectService);

            var url = await client.GetSignInUrl().ConfigureAwait(false);

            Assert.Contains(defaultSignInRedirectUrl, url, StringComparison.InvariantCultureIgnoreCase);

        }

        [Fact]
        public async Task WhenGetSignOutUrlCalledWithParameterThenReturnUrlWithSuppliedRedirect()
        {
            var redirect = "RedirectFromChild";
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, openIdConnectService);

            var url = await client.GetSignOutUrl(redirect).ConfigureAwait(false);

            Assert.Contains(redirect, url, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task WhenGetSignOutUrlCalledWithoutParameterThenReturnUrlWithDefaultRedirect()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, openIdConnectService);

            var url = await client.GetSignOutUrl(string.Empty).ConfigureAwait(false);

            Assert.Contains(defaultSignOutRedirectUrl, url, StringComparison.InvariantCultureIgnoreCase);

        }

        [Fact]
        public async Task WhenGetRegisterUrlCalledWithoutParameterThenReturnUrlWithDefaultRedirect()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, openIdConnectService);

            var url = await client.GetRegisterUrl().ConfigureAwait(false);

            Assert.Contains(defaultSignInRedirectUrl, url, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task WhenValidateTokenCalledThenAttemptToValidateToken()
        {
            var client = new AzureB2CAuthClient(defaultSettings, tokenHandler, openIdConnectService);
            SecurityToken secToken;
            var token = await client.ValidateToken("token").ConfigureAwait(true);
            A.CallTo(() => tokenHandler.ValidateToken(A<string>.Ignored,
                A<TokenValidationParameters>.That.Matches(x => x.ValidIssuer == defaultSettings.Value.Issuer),
                out secToken)).MustHaveHappened();
        }

        [Fact]
        public async Task
            WhenValidateTokenCalledAndUseOidConfigDiscoveryThenAttemptToValidateTokenUsingDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = defaultSignInRedirectUrl,
                SignOutRedirectUrl = defaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var config = new OpenIdConnectConfig
            {
                Issuer = "issuerFromServer",
                AuthorizationEndpoint = "AuthorizeUrl",
                JwksUri = "jwksUri",
                EndSessionEndpoint = "Endsesison",
                TokenEndpoint = "tokenEndpoint",
            };

            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).Returns(config);
            A.CallTo(() => openIdConnectService.GetJwkKey()).Returns(settings.Value.JWK);

            var client = new AzureB2CAuthClient(settings, tokenHandler, openIdConnectService);

            SecurityToken secToken;
            var token = await client.ValidateToken("token").ConfigureAwait(true);
            A.CallTo(() => tokenHandler.ValidateToken(A<string>.Ignored,
                    A<TokenValidationParameters>.That.Matches(x => x.ValidIssuer == settings.Value.Issuer),
                    out secToken))
                .MustHaveHappened();
        }

        [Fact]
        public async Task WhenGetSignOutCalledAndUseOidConfigDiscoveryThenUseDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = defaultSignInRedirectUrl,
                SignOutRedirectUrl = defaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var config = new OpenIdConnectConfig
            {
                Issuer = "issuerFromServer",
                AuthorizationEndpoint = "AuthorizeUrl",
                JwksUri = "jwksUri",
                EndSessionEndpoint = "Endsesison",
                TokenEndpoint = "tokenEndpoint",
            };

            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).Returns(config);

            var client = new AzureB2CAuthClient(settings, tokenHandler, openIdConnectService);

            var token = await client.GetSignOutUrl("test").ConfigureAwait(true);
            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).MustHaveHappened();
        }

        [Fact]
        public async Task WhenGetSignInCalledAndUseOidConfigDiscoveryThenUseDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = defaultSignInRedirectUrl,
                SignOutRedirectUrl = defaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var config = new OpenIdConnectConfig
            {
                Issuer = "issuerFromServer",
                AuthorizationEndpoint = "AuthorizeUrl",
                JwksUri = "jwksUri",
                EndSessionEndpoint = "Endsesison",
                TokenEndpoint = "tokenEndpoint",
            };

            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).Returns(config);

            var client = new AzureB2CAuthClient(settings, tokenHandler, openIdConnectService);

            var token = await client.GetSignInUrl().ConfigureAwait(true);
            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).MustHaveHappened();
        }

        [Fact]
        public async Task WhenGetRegisterUrlCalledAndUseOidConfigDiscoveryThenUseDiscoverySettings()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = defaultSignInRedirectUrl,
                SignOutRedirectUrl = defaultSignOutRedirectUrl,
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var config = new OpenIdConnectConfig
            {
                Issuer = "issuerFromServer",
                AuthorizationEndpoint = "AuthorizeUrl",
                JwksUri = "jwksUri",
                EndSessionEndpoint = "Endsesison",
                TokenEndpoint = "tokenEndpoint",
            };

            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).Returns(config);

            var client = new AzureB2CAuthClient(settings, tokenHandler, openIdConnectService);

            var token = await client.GetRegisterUrl().ConfigureAwait(true);
            A.CallTo(() => openIdConnectService.GetOpenIDConnectConfig()).MustHaveHappened();
        }
    }
}
