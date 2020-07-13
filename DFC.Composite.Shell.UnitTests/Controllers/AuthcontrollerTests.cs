using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.UnitTests.ClientHandlers;
using DFC.Composite.Shell.Utilities;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly IOpenIdConnectClient authClient;
        private readonly ILogger<AuthController> log;
        private readonly DefaultHttpContext defaultContext;
        private readonly IOptions<AuthSettings> defaultsettings;
        private readonly IAuthenticationService defaultAuthService;
        private readonly IVersionedFiles defaultVersionedFiles;
        private readonly IConfiguration defaultConfiguration;

        public AuthControllerTests()
        {
            authClient = A.Fake<IOpenIdConnectClient>();
            log = A.Fake<ILogger<AuthController>>();
            defaultVersionedFiles = A.Fake<IVersionedFiles>();
            defaultConfiguration = A.Fake<IConfiguration>();
            var requestServices = A.Fake<IServiceProvider>();
            defaultAuthService = A.Fake<IAuthenticationService>();
            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).Returns(Task.CompletedTask);

            A.CallTo(() => requestServices.GetService(typeof(IAuthenticationService))).Returns(defaultAuthService);

            defaultContext = new DefaultHttpContext
            {
                RequestServices = requestServices,
                Session = new MockHttpSession(),
            };

            defaultsettings = Options.Create(new AuthSettings
            {
                Audience = "audience",
                ClientSecret = "clientSecret123456",
                Issuer = "issuer",
                DefaultRedirectUrl = "test",
                AuthDssEndpoint = "test/{url}",
            });
        }

        [Fact]
        public async Task WhenSignInCalledWithOutRedirectUrlThenRedirectToLoginWithDefaultUrl()
        {
            A.CallTo(() => authClient.GetSignInUrl()).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>())),
                        Session = new MockHttpSession(),
                    },
                },
            };

            var result = await controller.SignIn(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetSignInUrl()).MustHaveHappened();
            Assert.Equal("test", result.Url);
        }

        [Fact]
        public async Task WhenSignOutCalledWithOutRedirectUrlThenRedirectToLoginWithDefaultUrl()
        {
            A.CallTo(() => authClient.GetSignOutUrl(string.Empty)).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };
            var result = await controller.SignOut(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetSignOutUrl(string.Empty)).MustHaveHappened();
            Assert.Equal("test", result.Url);
        }

        [Fact]
        public async Task WhenRegisterCalledWithOutRedirectUrlThenRedirectToLoginWithDefaultUrl()
        {
            A.CallTo(() => authClient.GetRegisterUrl()).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration);

            var result = await controller.Register(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetRegisterUrl()).MustHaveHappened();
            Assert.Equal("test", result.Url);
        }

        [Fact]
        public async Task WhenAuthCalledThenTokenIsValidated()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("customerId", "customerId"),
                new Claim("email", "email"),
                new Claim("given_name", "given_name"),
                new Claim("family_name", "family_name"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            await controller.Auth(token).ConfigureAwait(false);

            A.CallTo(() => authClient.ValidateToken(token)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenAuthCalledAndTokenIsInvalidThenThrowError()
        {
            var token = "token";
            A.CallTo(() => authClient.ValidateToken(token)).Throws(new Exception());

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            await Assert.ThrowsAsync<Exception>(async () => await controller.Auth(token).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenAuthCalledThenCookieIsCreated()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("customerId", "customerId"),
                new Claim("email", "email"),
                new Claim("given_name", "given_name"),
                new Claim("family_name", "family_name"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            await controller.Auth(token).ConfigureAwait(false);

            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenSignOutCalledThenCookieIsRemoved()
        {
            A.CallTo(() => authClient.GetSignOutUrl(string.Empty)).Returns("test");
            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            await controller.SignOut(string.Empty).ConfigureAwait(false);

            A.CallTo(() => defaultAuthService.SignOutAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<AuthenticationProperties>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenAuthCalledThenRedirectToSessionUrl()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("customerId", "customerId"),
                new Claim("email", "email"),
                new Claim("given_name", "given_name"),
                new Claim("family_name", "family_name"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };
            defaultContext.HttpContext.Session.SetString(AuthController.RedirectSessionKey, AuthController.RedirectSessionKey);

            var result = await controller.Auth(token).ConfigureAwait(false) as RedirectResult;

            Assert.Equal(result.Url, defaultsettings.Value.AuthDssEndpoint.Replace(AuthController.RedirectAttribute, AuthController.RedirectSessionKey, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task WhenAuthCalledAndSessionRedirectNotFoundThenRedirectToDefaultUrl()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("customerId", "customerId"),
                new Claim("email", "email"),
                new Claim("given_name", "given_name"),
                new Claim("family_name", "family_name"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            var result = await controller.Auth(token).ConfigureAwait(false) as RedirectResult;

            Assert.Equal(result.Url, defaultsettings.Value.AuthDssEndpoint.Replace(AuthController.RedirectAttribute, defaultsettings.Value.DefaultRedirectUrl, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
