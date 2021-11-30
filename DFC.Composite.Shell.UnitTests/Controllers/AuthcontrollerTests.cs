using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.UnitTests.ClientHandlers;
using DFC.Composite.Shell.Utilities;

using FakeItEasy;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private const string RefererUrl = "https://www.TestRefere.com";
        private const string BaseAddress = "www.test.com";
        private readonly IOpenIdConnectClient authClient;
        private readonly ILogger<AuthController> log;
        private readonly DefaultHttpContext defaultContext;
        private readonly IOptions<AuthSettings> defaultsettings;
        private readonly IAuthenticationService defaultAuthService;
        private readonly IVersionedFiles defaultVersionedFiles;
        private readonly IConfiguration defaultConfiguration;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;
        private readonly MockHttpSession session;
        private readonly IBaseUrlService baseUrlService;
        private readonly IUrlHelper defaultUrlHelper;

        public AuthControllerTests()
        {
            authClient = A.Fake<IOpenIdConnectClient>();
            log = A.Fake<ILogger<AuthController>>();
            defaultVersionedFiles = A.Fake<IVersionedFiles>();
            defaultConfiguration = A.Fake<IConfiguration>();
            var requestServices = A.Fake<IServiceProvider>();
            defaultAuthService = A.Fake<IAuthenticationService>();
            session = new MockHttpSession();
            baseUrlService = A.Fake<IBaseUrlService>();
            defaultUrlHelper = A.Fake<IUrlHelper>();
            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).Returns(Task.CompletedTask);

            A.CallTo(() => requestServices.GetService(typeof(IAuthenticationService))).Returns(defaultAuthService);

            A.CallTo(() => baseUrlService.GetBaseUrl(A<HttpRequest>.Ignored, A<IUrlHelper>.Ignored))
                .Returns(BaseAddress);

            defaultContext = new DefaultHttpContext
            {
                RequestServices = requestServices,
                Session = session,
                Request = { Headers = { new KeyValuePair<string, StringValues>("Referer", RefererUrl) } },
            };

            defaultsettings = Options.Create(new AuthSettings
            {
                Audience = "audience",
                ClientSecret = "clientSecret123456",
                Issuer = "issuer",
                DefaultRedirectUrl = "test",
                AuthDssEndpoint = "test/{url}",
            });

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
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
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

            var result = await controller.SignIn(string.Empty) as RedirectResult;

            A.CallTo(() => authClient.GetSignInUrl()).MustHaveHappened();
            Assert.Null(session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenResetPasswordCalledWithThenDoNotSetSessionRedirect()
        {
            A.CallTo(() => authClient.GetResetPasswordUrl()).Returns("test");
            var settings = Options.Create(new AuthSettings());
            var session = new MockHttpSession();
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
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

            var result = await controller.ResetPassword() as RedirectResult;

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
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
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
            var result = await controller.SignIn(string.Empty) as RedirectResult;

            Assert.Equal(redirectUrl, session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenSignInCalledWithRedirectUrlThenSetToRredirctUrl()
        {
            A.CallTo(() => authClient.GetSignInUrl()).Returns("test");
            var session = new MockHttpSession();
            var redirectUrl = "Redirect.com";
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
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
            var result = await controller.SignIn(redirectUrl) as RedirectResult;

            Assert.Equal(redirectUrl, session.GetString(AuthController.RedirectSessionKey));
        }

        [Fact]
        public async Task WhenSignOutCalledWithOutRedirectUrlThenRedirectToRedirectUrl()
        {
            A.CallTo(() => authClient.GetSignOutUrl(A<string>.Ignored)).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
                Url = defaultUrlHelper,
            };
            var redirecturl = "/redirect";
            var result = await controller.SignOut(redirecturl) as RedirectResult;

            A.CallTo(() => authClient.GetSignOutUrl(BaseAddress + redirecturl)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenSignOutCalledWithOutRedirectUrlAndRefererIsNotNullThenRedirectToRefererUrl()
        {
            A.CallTo(() => authClient.GetSignOutUrl(A<string>.Ignored)).Returns("test");
            var settings = Options.Create(new AuthSettings());
            using var controller = new AuthController(authClient, log, settings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
                Url = defaultUrlHelper,
            };
            var result = await controller.SignOut(string.Empty) as RedirectResult;

            A.CallTo(() => authClient.GetSignOutUrl(RefererUrl)).MustHaveHappened();
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
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            controller.Url = new UrlHelper(
                new ActionContext(defaultContext, new RouteData(), new ActionDescriptor()));

            await controller.Auth(token);

            A.CallTo(() => authClient.ValidateToken(token)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenAuthCalledAndTokenIsInvalidThenThrowError()
        {
            var token = "token";
            A.CallTo(() => authClient.ValidateToken(token)).Throws(new Exception());

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            await Assert.ThrowsAsync<Exception>(async () => await controller.Auth(token));
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
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };

            controller.Url = new UrlHelper(new ActionContext(defaultContext, new RouteData(), new ActionDescriptor()));

            await controller.Auth(token);

            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenSignOutCalledThenCookieIsRemoved()
        {
            A.CallTo(() => authClient.GetSignOutUrl(A<string>.Ignored)).Returns("test");
            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
                Url = defaultUrlHelper,
            };

            await controller.SignOut(string.Empty);

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
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };
            defaultContext.HttpContext.Session.SetString(AuthController.RedirectSessionKey, AuthController.RedirectSessionKey);

            controller.Url = new UrlHelper(new ActionContext(defaultContext, new RouteData(), new ActionDescriptor()));
            var result = await controller.Auth(token) as RedirectResult;

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
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            using var controller = new AuthController(authClient, log, defaultsettings, defaultVersionedFiles, defaultConfiguration, baseUrlService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = defaultContext,
                },
            };
            controller.Url = new UrlHelper(new ActionContext(defaultContext, new RouteData(), new ActionDescriptor()));

            var result = await controller.Auth(token) as RedirectResult;

            Assert.Equal(result.Url, defaultsettings.Value.AuthDssEndpoint.Replace(AuthController.RedirectAttribute, defaultsettings.Value.DefaultRedirectUrl, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
