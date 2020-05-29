using DFC.Composite.Shell.Services.Auth.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Auth
{
    public class OpenIdConnectService : IOpenIdConnectService
    {
        private readonly OpenIDConnectSettings settings;
        private readonly HttpClient client;

        public OpenIdConnectService(IOptions<OpenIDConnectSettings> settings, HttpClient client)
        {
            this.settings = settings?.Value;
            this.client = client;
        }

        public async Task<OpenIdConnectConfig> GetOpenIDConnectConfig()
        {
            using var msg = new HttpRequestMessage(HttpMethod.Get, settings.OIDCConfigMetaDataUrl);
            var response = await client.SendAsync(msg).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<OpenIdConnectConfig>().ConfigureAwait(false);
        }

        public async Task<string> GetJwkKey()
        {
            JsonWebKey[] keyArray;
            using (var msg = new HttpRequestMessage(HttpMethod.Get, settings.JWKsUrl))
            {
                var response = await client.SendAsync(msg).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var jobject = await response.Content.ReadAsAsync<JObject>().ConfigureAwait(false);
                var keys = jobject["keys"];
                keyArray = keys.ToObject<JsonWebKey[]>();
            }

            return keyArray[0].N;
        }
    }
}
