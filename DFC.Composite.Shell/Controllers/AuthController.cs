using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.Services.BaseUrl;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IBaseUrlService baseUrlService;

        public AuthController(
            IOpenIdConnectClient authClient,
            ILogger<AuthController> logger,
            IOptions<AuthSettings> settings,
            IBaseUrlService baseUrlService)
        {
            this.authClient = authClient;
            this.logger = logger;
            this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            this.baseUrlService = baseUrlService;
        }

        public async Task<IActionResult> SignIn(string redirectUrl)
        {
            SetSessionRedirectUrl(GetRequestRedirectURl(redirectUrl));
            var signInUrl = await authClient.GetSignInUrl();

            return Redirect(signInUrl);
        }

        public async Task<IActionResult> ResetPassword()
        {
            var resetPasswordUrl = await authClient.GetResetPasswordUrl();
            return Redirect(resetPasswordUrl);
        }

        public async Task<IActionResult> SignOut(string redirectUrl)
        {
            var url = GenerateSignOutUrl(redirectUrl);
            var signOutUrl = await authClient.GetSignOutUrl(url);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect(signOutUrl);
        }

        public async Task<IActionResult> Auth(string id_token)
        {
            JwtSecurityToken validatedToken;

            try
            {
                validatedToken = await authClient.ValidateToken(id_token);
            }
            catch (Exception ex)
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

            var authProperties = new AuthenticationProperties
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
                            new Claim(ClaimTypes.Name, $"{GetClaimValue(validatedToken.Claims, "given_name")} {GetClaimValue(validatedToken.Claims, "family_name")}"),
                        },
                        CookieAuthenticationDefaults.AuthenticationScheme)), authProperties);

            return Redirect(GetAndResetRedirectUrl());
        }

        private static string GetClaimValue(IEnumerable<Claim> claims, string name)
        {
            return claims.FirstOrDefault(claim => claim.Type == name)?.Value;
        }

        private static bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
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

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private void SetSessionRedirectUrl(string redirectUrl)
        {
            redirectUrl = string.IsNullOrEmpty(redirectUrl) ? GetReferer() : redirectUrl;

            if (string.IsNullOrEmpty(redirectUrl))
            {
                return;
            }

            HttpContext.Session.SetString(RedirectSessionKey, redirectUrl);
        }

        private string GetRequestRedirectURl(string redirectFromQuery)
        {
            var referer = GetReferer();

            var refererMissing = string.IsNullOrEmpty(referer);
            var redirectToMissing = string.IsNullOrEmpty(redirectFromQuery);
            var useDefaultRedirectUrl = refererMissing && redirectToMissing;

            return useDefaultRedirectUrl ? settings.DefaultRedirectUrl?.ToString() : redirectToMissing ? referer : redirectFromQuery;
        }

        private string GetAndResetRedirectUrl()
        {
            var url = HttpContext.Session.GetString(RedirectSessionKey);
            var redirectUrl = string.IsNullOrEmpty(url) ? settings.DefaultRedirectUrl.ToString() : url;

            HttpContext.Session.Remove(RedirectSessionKey);
            return settings.AuthDssEndpoint.Replace(RedirectAttribute, redirectUrl, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GenerateSignOutUrl(string redirectUrl)
        {
            var redirectUrlOrReferer = string.IsNullOrEmpty(redirectUrl) ? GetReferer() : redirectUrl;

            return IsAbsoluteUrl(redirectUrlOrReferer) ? redirectUrlOrReferer
                : baseUrlService.GetBaseUrl(Request, Url) + redirectUrlOrReferer;
        }

        private string GetReferer()
        {
            return Request.GetTypedHeaders().Referer?.ToString();
        }
    }
}
