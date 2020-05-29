using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DFC.Composite.Shell.Services.Auth;
using Xunit;

namespace DFC.Composite.Shell.UnitTests.ServicesTests
{
    public class OpenIdConnectServiceTests
    {

        public OpenIdConnectServiceTests()
        {
        }

        [Fact]
        public async Task WhenGetOpenIDConnectConfigCallThenReturnConfig()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = "test",
                SignOutRedirectUrl = "test",
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var config = new OpenIdConnectConfig
            {
                Issuer = "issuerFromServer",
                AuthorizationEndpoint = "AuthorizeUrl",
                JwksUri = "jwksUri",
                EndSessionEndpoint = "Endsesison",
                TokenEndpoint = "tokenEndpoint",
            };

            var redirectHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(config), System.Text.Encoding.UTF8, "application/json"),
            };
            redirectHttpResponse.Headers.Location = new Uri("http://SomeLocation");

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(redirectHttpResponse);
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);

            var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            var service = new OpenIdConnectService(settings, httpClient);

            var results = await service.GetOpenIDConnectConfig().ConfigureAwait(false);

            Assert.Equal(results.AuthorizationEndpoint, config.AuthorizationEndpoint);

            redirectHttpResponse.Dispose();
            httpClient.Dispose();

        }

        [Fact]
        public async Task WhenGetJwkKeyCalledThenReturnJwkKey()
        {
            var settings = Options.Create(new OpenIDConnectSettings
            {
                UseOIDCConfigDiscovery = true,
                OIDCConfigMetaDataUrl = "test",
                RedirectUrl = "test",
                SignOutRedirectUrl = "test",
                Issuer = "issuerFromServer",
                AuthdUrl = "auth",
                AuthorizeUrl = "AuthorizeUrl",
                ClientId = "clientid",
                EndSessionUrl = "Endsesison",
                JWK = "jjjjjjfhfjjfjfjfjfhfjkhdfkhdfkjhskfhsldkjhfskdljfhsdlkfhsdflksdhsdlkfh",
            });

            var config = new OpenIdConnectConfig
            {
                Issuer = "issuerFromServer",
                AuthorizationEndpoint = "AuthorizeUrl",
                JwksUri = "jwksUri",
                EndSessionEndpoint = "Endsesison",
                TokenEndpoint = "tokenEndpoint",
            };
            var jwt = "test";
            var redirectHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"keys\":[{\"n\":\"" + jwt + "\"}]}"),
            };
            redirectHttpResponse.Headers.Location = new Uri("http://SomeLocation");
            redirectHttpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(redirectHttpResponse);
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);

            var httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };

            var service = new OpenIdConnectService(settings, httpClient);

            var results = await service.GetJwkKey().ConfigureAwait(false);
            Assert.Equal(results, jwt);

            redirectHttpResponse.Dispose();
            httpClient.Dispose();

        }
    }
}
