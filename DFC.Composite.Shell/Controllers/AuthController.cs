using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using Microsoft.AspNetCore.Authentication;
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
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DFC.Composite.Shell.Controllers
{
    public class AuthController : Controller
    {
        public const string RedirectSessionKey = "RedirectSession";
        private readonly IOpenIdConnectClient authClient;
        private readonly ILogger<AuthController> logger;
        private readonly AuthSettings settings;

        public AuthController(IOpenIdConnectClient client, ILogger<AuthController> logger, IOptions<AuthSettings> settings)
        {
            authClient = client;
            this.logger = logger;
            this.settings = settings.Value;
        }

        public async Task<IActionResult> SignIn(string redirectUrl)
        {
            SetRedirectUrl(redirectUrl);
            var signInUrl = await authClient.GetSignInUrl().ConfigureAwait(false);
            return Redirect(signInUrl);
        }

        public async Task<IActionResult> SignOut(string redirectUrl)
        {
            SetRedirectUrl(redirectUrl);
            var signInUrl = await authClient.GetSignOutUrl(redirectUrl).ConfigureAwait(false);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return Redirect(signInUrl);
        }

        public async Task<IActionResult> Auth(string id_token, string state)
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
                new Claim("CustomerId", validatedToken.Claims.FirstOrDefault(claim => claim.Type == "tid")?.Value),
                new Claim("email", validatedToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value),
            };

            var expiryTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            expiryTime = expiryTime.AddSeconds(double.Parse(validatedToken.Claims.First(claim => claim.Type == "exp").Value, new DateTimeFormatInfo()));
            var authProperties = new AuthenticationProperties()
            {
                AllowRefresh = false,
                ExpiresUtc = expiryTime,
                IsPersistent = false,
            };

            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("bearer", CreateChildAppToken(claims, expiryTime)) }, CookieAuthenticationDefaults.AuthenticationScheme)), authProperties).ConfigureAwait(false);

            return Redirect(GetRedirectUrl());
        }

        public async Task<IActionResult> Register(string redirectUrl)
        {
            SetRedirectUrl(redirectUrl);
            var signInUrl = await authClient.GetRegisterUrl().ConfigureAwait(false);
            return Redirect(signInUrl);
        }

        private string CreateChildAppToken(List<Claim> claims, DateTime expiryTime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

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
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                HttpContext.Session.SetString(RedirectSessionKey, redirectUrl);
            }
        }

        private string GetRedirectUrl()
        {
            var url = HttpContext.Session.GetString(RedirectSessionKey);
            return string.IsNullOrEmpty(url) ? settings.DefaultRedirectUrl : url;
        }
    }
}