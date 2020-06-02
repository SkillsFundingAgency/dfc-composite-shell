using DFC.Composite.Shell.Services.Auth.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Auth
{
    public class AzureB2CAuthClient : IOpenIdConnectClient
    {
        private readonly OpenIDConnectSettings settings;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;
        private readonly SecurityTokenHandler tokenHandler;

        public AzureB2CAuthClient(IOptions<OpenIDConnectSettings> settings, SecurityTokenHandler securityTokenHandler, IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this.settings = settings.Value;
            this.configurationManager = configurationManager;
            tokenHandler = securityTokenHandler;
        }

        public async Task<string> GetRegisterUrl()
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("p", "B2C_1A_account_signup");
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("nonce", "defaultNonce");
            queryParams.Add("redirect_uri", settings.RedirectUrl);
            queryParams.Add("scope", "openid");
            queryParams.Add("response_type", "id_token");
            queryParams.Add("prompt", "login");
            string registerUrl = QueryHelpers.AddQueryString(GetUrlWithoutParams(configDoc.AuthorizationEndpoint), queryParams);

            return registerUrl;
        }

        public async Task<string> GetSignInUrl()
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("p", "B2C_1A_signin_invitation");
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("nonce", "defaultNonce");
            queryParams.Add("redirect_uri", settings.RedirectUrl);
            queryParams.Add("scope", "openid");
            queryParams.Add("response_type", "id_token");
            queryParams.Add("response_mode", "query");
            queryParams.Add("prompt", "login");
            string registerUrl = QueryHelpers.AddQueryString(GetUrlWithoutParams(configDoc.AuthorizationEndpoint), queryParams);

            return registerUrl;
        }

        public async Task<string> GetSignOutUrl(string redirectUrl)
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("post_logout_redirect_uri", string.IsNullOrEmpty(redirectUrl) ? settings.SignOutRedirectUrl : redirectUrl);
            string registerUrl = QueryHelpers.AddQueryString(configDoc.EndSessionEndpoint, queryParams);

            return registerUrl;
        }

        public async Task<JwtSecurityToken> ValidateToken(string token)
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
            return await ValidateToken(token, configDoc).ConfigureAwait(false);
        }

        private static string GetUrlWithoutParams(string url)
        {
            return new UriBuilder(url) { Query = string.Empty }.ToString();
        }

        private async Task<JwtSecurityToken> ValidateToken(
            string token,
            OpenIdConnectConfiguration discoveryDocument,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrEmpty(discoveryDocument.Issuer)) throw new ArgumentNullException(nameof(discoveryDocument.Issuer));

            var signingKeys = discoveryDocument.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = discoveryDocument.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ValidateAudience = false,
                ClockSkew = TimeSpan.FromMinutes(2),
            };

            tokenHandler.ValidateToken(token, validationParameters, out var rawValidatedToken);

            return (JwtSecurityToken)rawValidatedToken;
        }
    }
}
