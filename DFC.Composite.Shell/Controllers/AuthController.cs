using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class AuthController : Controller
    {
        public const string RedirectSessionKey = "RedirectSession";
        public const string RedirectAttribute = "{url}";

        private readonly IOpenIdConnectClient authClient;
        private readonly ILogger<AuthController> logger;
        private readonly AuthSettings settings;
        private readonly IVersionedFiles versionedFiles;
        private readonly IConfiguration configuration;

        public AuthController(IOpenIdConnectClient client, ILogger<AuthController> logger, IOptions<AuthSettings> settings, IVersionedFiles versionedFiles, IConfiguration configuration)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            authClient = client;
            this.logger = logger;
            this.settings = settings.Value;
            this.versionedFiles = versionedFiles;
            this.configuration = configuration;
        }

        public async Task<IActionResult> SignIn(string redirectUrl)
        {
            SetRedirectUrl(redirectUrl);
            var signInUrl = await authClient.GetSignInUrl().ConfigureAwait(false);
            return Redirect(signInUrl);
        }

        public async Task<IActionResult> SignOut(string redirectUrl)
        {
            redirectUrl = string.IsNullOrEmpty(redirectUrl) ? Request.Headers["Referer"].ToString() : redirectUrl;
            var signInUrl = await authClient.GetSignOutUrl(redirectUrl).ConfigureAwait(false);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Redirect(signInUrl);
        }

        public async Task<IActionResult> Auth(string id_token)
        {
            JwtSecurityToken validatedToken;
            try
            {
                validatedToken = await authClient.ValidateToken(id_token).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Failed to validate auth token.");
                throw;
            }

            var claims = new List<Claim>
            {
                new Claim("CustomerId", validatedToken.Claims.FirstOrDefault(claim => claim.Type == "customerId")?.Value),
                new Claim(ClaimTypes.Email, validatedToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value),
                new Claim(ClaimTypes.GivenName, validatedToken.Claims.FirstOrDefault(claim => claim.Type == "given_name")?.Value),
                new Claim(ClaimTypes.Surname, validatedToken.Claims.FirstOrDefault(claim => claim.Type == "family_name")?.Value),
                new Claim("DssToken", id_token),
            };

            var expiryTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            expiryTime = expiryTime.AddSeconds(double.Parse(validatedToken.Claims.First(claim => claim.Type == "exp").Value, new DateTimeFormatInfo()));
            var authProperties = new AuthenticationProperties()
            {
                AllowRefresh = false,
                ExpiresUtc = expiryTime,
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                new ClaimsPrincipal(
                new ClaimsIdentity(
                    new List<Claim>
                    {
                        new Claim("bearer", CreateChildAppToken(claims, expiryTime)),
                        new Claim(ClaimTypes.Name, $"{validatedToken.Claims.FirstOrDefault(claim => claim.Type == "given_name")?.Value} {validatedToken.Claims.FirstOrDefault(claim => claim.Type == "family_name")?.Value}"),
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme)), authProperties).ConfigureAwait(false);

            return Redirect(GetAndResetRedirectUrl());
        }

        private string CreateChildAppToken(List<Claim> claims, DateTime expiryTime)
        {
            var now = DateTime.UtcNow;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.ClientSecret));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                notBefore: now,
                expires: expiryTime,
                signingCredentials: credentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private void SetRedirectUrl(string redirectUrl)
        {
            redirectUrl = string.IsNullOrEmpty(redirectUrl) ? Request.Headers["Referer"].ToString() : redirectUrl;
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                HttpContext.Session.SetString(RedirectSessionKey, redirectUrl);
            }
        }

        private string GetAndResetRedirectUrl()
        {
            var url = HttpContext.Session.GetString(RedirectSessionKey);
            var redirectUrl = string.IsNullOrEmpty(url) ? settings.DefaultRedirectUrl : url;
            HttpContext.Session.Remove(RedirectSessionKey);
            return settings.AuthDssEndpoint.Replace(RedirectAttribute, redirectUrl, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}