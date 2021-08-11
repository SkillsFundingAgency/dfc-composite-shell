using DFC.Composite.Shell.Services.TokenRetriever;

using FakeItEasy;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class BearerTokenRetrieverTests
    {
        [Fact]
        public async Task GetTokenReturnsTokenValue()
        {
            const string TokenValue = "SomeTokenValue";

            var service = new BearerTokenRetriever();
            var context = new DefaultHttpContext();
            var serviceProvider = A.Fake<IServiceProvider>();
            var fakeAuthenticationService = A.Fake<IAuthenticationService>();
            A.CallTo(() => fakeAuthenticationService.AuthenticateAsync(A<HttpContext>.Ignored, A<string>.Ignored))
                .Returns(AuthenticateResult.Success(
                    new AuthenticationTicket(
                        new ClaimsPrincipal(),
                        new AuthenticationProperties
                        {
                            Items = { new KeyValuePair<string, string>(".Token.id_token", TokenValue) },
                        },
                        "SomeScheme")));

            A.CallTo(() => serviceProvider.GetService(typeof(IAuthenticationService))).Returns(fakeAuthenticationService);
            context.RequestServices = serviceProvider;

            var result = await service.GetToken(context);

            Assert.Equal(TokenValue, result);
        }
    }
}