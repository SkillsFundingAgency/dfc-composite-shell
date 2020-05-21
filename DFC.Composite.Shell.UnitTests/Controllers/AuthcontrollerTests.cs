﻿using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.UnitTests.ClientHandlers;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public AuthControllerTests()
        {
            authClient = A.Fake<IOpenIdConnectClient>();
            log = A.Fake<ILogger<AuthController>>();
            var requestServices = A.Fake<IServiceProvider>();
            defaultAuthService = A.Fake<IAuthenticationService>();
            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored,
                A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).Returns(Task.CompletedTask);

            A.CallTo(() => requestServices.GetService(typeof(IAuthenticationService))).Returns(defaultAuthService);

            defaultContext = new DefaultHttpContext
            {
                RequestServices = requestServices,
                Session = new MockHttpSession()
            };

            defaultsettings = Options.Create(new AuthSettings
            {
                Audience = "audience",
                ClientSecret = "clientSecret123456",
                Issuer = "issuer",
                DefaultRedirectUrl = "test",
            });
        }

        [Fact]
        public async Task WhenSignInCalledWithOutRedirectUrlThenRedirectToLoginWithDefaultUrl()
        {
            A.CallTo(() => authClient.GetSignInUrl()).Returns("test");
            var settings = Options.Create(new AuthSettings());
            var controller = new AuthController(authClient, log, settings);

            var result = await controller.SignIn(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetSignInUrl()).MustHaveHappened();
            Assert.Equal("test", result.Url);
            controller.Dispose();
        }

        [Fact]
        public async Task WhenSignOutCalledWithOutRedirectUrlThenRedirectToLoginWithDefaultUrl()
        {
            A.CallTo(() => authClient.GetSignOutUrl(string.Empty)).Returns("test");
            var settings = Options.Create(new AuthSettings());
            var controller = new AuthController(authClient, log, settings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
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
            var controller = new AuthController(authClient, log, settings);

            var result = await controller.Register(string.Empty).ConfigureAwait(false) as RedirectResult;

            A.CallTo(() => authClient.GetRegisterUrl()).MustHaveHappened();
            Assert.Equal("test", result.Url);
            controller.Dispose();
        }

        [Fact]
        public async Task WhenAuthCalledThenTokenIsValidated()
        {
            var token = "token";
            var claims = new List<Claim>
            {
            new Claim("tid", "tid"),
            new Claim("email", "email"),
            new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));
            
            var controller = new AuthController(authClient, log, defaultsettings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
            };
          
            await controller.Auth(token, "state").ConfigureAwait(false);

            A.CallTo(() => authClient.ValidateToken(token)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenAuthCalledAndTokenIsInvalidThenThrowError()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("tid", "tid"),
                new Claim("email", "email"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Throws(new Exception());

            var controller = new AuthController(authClient, log, defaultsettings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
            };

            await Assert.ThrowsAsync<Exception>(async () => await controller.Auth(token, "state").ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public async Task WhenAuthCalledThenCookieIsCreated()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("tid", "tid"),
                new Claim("email", "email"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            var controller = new AuthController(authClient, log, defaultsettings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
            };

            await controller.Auth(token, "state").ConfigureAwait(false);

            A.CallTo(() => defaultAuthService.SignInAsync(A<HttpContext>.Ignored, A<string>.Ignored, A<ClaimsPrincipal>.Ignored, A<AuthenticationProperties>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenSignOutCalledThenCookieIsRemoved()
        {
            A.CallTo(() => authClient.GetSignOutUrl(string.Empty)).Returns("test");
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("tid", "tid"),
                new Claim("email", "email"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };

            var controller = new AuthController(authClient, log, defaultsettings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
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
                new Claim("tid", "tid"),
                new Claim("email", "email"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            var controller = new AuthController(authClient, log, defaultsettings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
            };
            defaultContext.HttpContext.Session.SetString(AuthController.RedirectSessionKey, AuthController.RedirectSessionKey);

            var result = await controller.Auth(token, "state").ConfigureAwait(false) as RedirectResult;

            Assert.Equal(result.Url, AuthController.RedirectSessionKey);
        }

        [Fact]
        public async Task WhenAuthCalledAndSessionRedirectNotFoundThenRedirectToDefaultUrl()
        {
            var token = "token";
            var claims = new List<Claim>
            {
                new Claim("tid", "tid"),
                new Claim("email", "email"),
                new Claim("exp", DateTimeOffset.Now.AddHours(2).ToUnixTimeSeconds().ToString()),
            };
            A.CallTo(() => authClient.ValidateToken(token)).Returns(new JwtSecurityToken("test", "test", claims));

            var controller = new AuthController(authClient, log, defaultsettings);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = defaultContext,
            };

            var result = await controller.Auth(token, "state").ConfigureAwait(false) as RedirectResult;

            Assert.Equal(result.Url, defaultsettings.Value.DefaultRedirectUrl);
        }
    }
}