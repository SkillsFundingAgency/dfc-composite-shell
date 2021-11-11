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
        public const string SignInRequestType = "B2C_1A_signin_invitation";
        public const string PasswordResetRequestType = "B2C_1A_Password_Reset";
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
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None);
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

        public Task<string> GetSignInUrl()
        {
            return GetAuthEndpoint(SignInRequestType);
        }

        public Task<string> GetResetPasswordUrl()
        {
            return GetAuthEndpoint(PasswordResetRequestType);
        }

        public async Task<string> GetSignOutUrl(string redirectUrl)
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("post_logout_redirect_uri", string.IsNullOrEmpty(redirectUrl) ? settings.SignOutRedirectUrl : redirectUrl);
            string registerUrl = QueryHelpers.AddQueryString(configDoc.EndSessionEndpoint, queryParams);

            return registerUrl;
        }

        public async Task<JwtSecurityToken> ValidateToken(string token)
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            return ValidateToken(token, configDoc);
        }

        private static string GetUrlWithoutParams(string url)
        {
            return new UriBuilder(url) { Query = string.Empty }.ToString();
        }

        private async Task<string> GetAuthEndpoint(string requestType)
        {
            var configDoc = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("p", requestType);
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

        private JwtSecurityToken ValidateToken(
            string token,
            OpenIdConnectConfiguration discoveryDocument,
            CancellationToken ct = default(CancellationToken))
        {
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
