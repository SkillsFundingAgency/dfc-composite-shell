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
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IOptions<OpenIDConnectSettings> defaultSettings;
        private readonly SecurityTokenHandler tokenHandler;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;
        private const string refererUrl = "TestRefere.com";
        private readonly MockHttpSession session;

        public AuthControllerTests()
        {
            authClient = A.Fake<IOpenIdConnectClient>();
            log = A.Fake<ILogger<AuthController>>();
            defaultVersionedFiles = A.Fake<IVersionedFiles>();
            defaultConfiguration = A.Fake<IConfiguration>();
            var requestServices = A.Fake<IServiceProvider>();
            defaultAuthService = A.Fake<IAuthenticationService>();
            session = new MockHttpSession();
            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).Returns(Task.CompletedTask);

            A.CallTo(() => requestServices.GetService(typeof(IAuthenticationService))).Returns(defaultAuthService);

            defaultContext = new DefaultHttpContext
            {
                RequestServices = requestServices,
                Session = session,
                Request = { Headers = { new KeyValuePair<string, StringValues>("Referer", refererUrl) } },
            };

            defaultsettings = Options.Create(new AuthSettings
            {
                Audience = "audience",
                ClientSecret = "clientSecret123456",
                Issuer = "issuer",
                DefaultRedirectUrl = "test",
                AuthDssEndpoint = "test/{url}",
            });

            defaultSettings = Options.Create(new OpenIDConnectSettings
            {
                RedirectUrl = "test/",
                SignOutRedirectUrl = "test/",
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
        public async Task WhenSignInCalledWithOutRedirectUrlThenDoNotSetSessionRedirect()
        {
            A.CallTo(() => authClient.GetSignInUrl()).Returns("test");
            var settings = Options.Create(new AuthSettings());
            var session = new MockHttpSession();
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>())),
                        Session = session,
                    },
                },
            };

            var result = await controller.SignIn(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetSignInUrl()).MustHaveHappened();
            Assert.Null(session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenResetPasswordCalledWithThenDoNotSetSessionRedirect()
        {
            A.CallTo(() => authClient.GetResetPasswordUrl()).Returns("test");
            var settings = Options.Create(new AuthSettings());
            var session = new MockHttpSession();
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>())),
                        Session = session,
                    },
                },
            };

            var result = await controller.ResetPassword().ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetResetPasswordUrl()).MustHaveHappened();
            Assert.Null(session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenSignInCalledWithOutRedirectUrlAndRefererIsNotNullThenSetSessionToRefererUrl()
        {
            A.CallTo(() => authClient.GetSignInUrl()).Returns("test");
            var session = new MockHttpSession();
            var redirectUrl = "test.com";
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>())),
                        Session = session,
                        Request = { Headers = { new KeyValuePair<string, StringValues>("Referer", redirectUrl) } },
                    },
                },
            };
            var result = await controller.SignIn(string.Empty).ConfigureAwait(false) as RedirectResult;

            Assert.Equal(redirectUrl, session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenSignInCalledWithRedirectUrlThenSetToRredirctUrl()
        {
            A.CallTo(() => authClient.GetSignInUrl()).Returns("test");
            var session = new MockHttpSession();
            var referer = "test.com";
            var redirectUrl = "Redirect.com";
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>())),
                        Session = session,
                        Request = { Headers = { new KeyValuePair<string, StringValues>("Referer", redirectUrl) } },
                    },
                },
            };
            var result = await controller.SignIn(redirectUrl).ConfigureAwait(false) as RedirectResult;

            Assert.Equal(redirectUrl, session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenSignOutCalledWithOutRedirectUrlThenRedirectToRedirectUrl()
        {
            A.CallTo(() => authClient.GetSignOutUrl(A<string>.Ignored)).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };
            var redirecturl = "redirect.com";
            var result = await controller.SignOut(redirecturl).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetSignOutUrl(redirecturl)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenSignOutCalledWithOutRedirectUrlAndRefererIsNotNullThenRedirectToRefererUrl()
        {
            A.CallTo(() => authClient.GetSignOutUrl(A<string>.Ignored)).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };
            var result = await controller.SignOut(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetSignOutUrl(refererUrl)).MustHaveHappened();
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
            A.CallTo(() => authClient.GetSignOutUrl(A<string>.Ignored)).Returns("test");
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
            Assert.Null(session.GetString(AuthController.RedirectSessionKey));
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
