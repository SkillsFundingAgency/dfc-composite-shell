using DFC.Composite.Shell.Services.Auth.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Auth
{
    public class AzureB2CAuthClient : IOpenIdConnectClient
    {
        private readonly OpenIDConnectSettings settings;
        private readonly SecurityTokenHandler tokenHandler;
        private readonly IOpenIdConnectService openIdConnectService;

        public AzureB2CAuthClient(IOptions<OpenIDConnectSettings> settings, SecurityTokenHandler securityTokenHandler, IOpenIdConnectService openIdConnectService)
        {
            this.settings = settings.Value;
            tokenHandler = securityTokenHandler;
            this.openIdConnectService = openIdConnectService;
        }

        public async Task<string> GetRegisterUrl()
        {
            if (settings.UseOIDCConfigDiscovery)
            {
                await LoadOpenIDConnectConfig().ConfigureAwait(false);
            }

            var queryParams = new Dictionary<string, string>();
            queryParams.Add("p", "B2C_1A_account_signup");
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("nonce", "defaultNonce");
            queryParams.Add("redirect_uri", settings.RedirectUrl);
            queryParams.Add("scope", "openid");
            queryParams.Add("response_type", "id_token");
            queryParams.Add("prompt", "login");
            string registerUrl = QueryHelpers.AddQueryString(settings.AuthorizeUrl, queryParams);

            return registerUrl;
        }

        public async Task<string> GetSignInUrl()
        {
            if (settings.UseOIDCConfigDiscovery)
            {
                await LoadOpenIDConnectConfig().ConfigureAwait(false);
            }

            var queryParams = new Dictionary<string, string>();
            queryParams.Add("p", "B2C_1A_signin_invitation");
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("nonce", "defaultNonce");
            queryParams.Add("redirect_uri", settings.RedirectUrl);
            queryParams.Add("scope", "openid");
            queryParams.Add("response_type", "id_token");
            queryParams.Add("response_mode", "query");
            queryParams.Add("prompt", "login");
            string registerUrl = QueryHelpers.AddQueryString(settings.AuthorizeUrl, queryParams);

            return registerUrl;
        }

        public async Task<string> GetSignOutUrl(string redirectUrl)
        {
            if (settings.UseOIDCConfigDiscovery)
            {
                await LoadOpenIDConnectConfig().ConfigureAwait(false);
            }

            var queryParams = new Dictionary<string, string>();
            queryParams.Add("client_id", settings.ClientId);
            queryParams.Add("post_logout_redirect_uri", string.IsNullOrEmpty(redirectUrl) ? settings.SignOutRedirectUrl : redirectUrl);
            string registerUrl = QueryHelpers.AddQueryString(settings.EndSessionUrl, queryParams);

            return registerUrl;
        }

        public async Task<JwtSecurityToken> ValidateToken(string token)
        {
            if (settings.UseOIDCConfigDiscovery)
            {
                await LoadOpenIDConnectConfig();
                await LoadJsonWebKeyAsync();
            }

            // Do we include Personally Identifiable Information in any exceptions or logging?
            IdentityModelEventSource.ShowPII = settings.LogPersonalInfo;

            // This will throw an exception if the token fails to validate.
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetRsaSecurityKey(),
                ValidateAudience = false,
            }, out SecurityToken validatedToken);

            return validatedToken as JwtSecurityToken;
        }

        private static string GetUrlWithoutParams(string url)
        {
            return new UriBuilder(url) { Query = string.Empty }.ToString();
        }

        private async Task LoadOpenIDConnectConfig()
        {
            var config = await openIdConnectService.GetOpenIDConnectConfig().ConfigureAwait(false);
            settings.AuthorizeUrl = GetUrlWithoutParams(config.AuthorizationEndpoint);
            settings.JWKsUrl = config.JwksUri;
            settings.Issuer = config.Issuer;
            settings.EndSessionUrl = config.EndSessionEndpoint;
        }

        private async Task LoadJsonWebKeyAsync()
        {
            settings.JWK = await openIdConnectService.GetJwkKey().ConfigureAwait(false);
        }

        private static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/", StringComparison.InvariantCultureIgnoreCase).Replace("-", "+", StringComparison.InvariantCultureIgnoreCase);
            return Convert.FromBase64String(base64);
        }

        private RsaSecurityKey GetRsaSecurityKey()
        {
            var rsa = new RSACryptoServiceProvider(2048);

            rsa.ImportParameters(
                new RSAParameters()
                {
                    Modulus = FromBase64Url(settings.JWK),
                    Exponent = FromBase64Url("AQAB"),
                });
            var rsaKey = new RsaSecurityKey(rsa);
            return rsaKey;
        }
    }
}
