using System;
using Newtonsoft.Json;

namespace DFC.Composite.Shell.Services.Auth.Models
{
    public class OpenIdConnectConfig
    {
        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonProperty("token_endpoint")]
        public string TokenEndpoint { get; set; }

        [JsonProperty("end_session_endpoint")]
        public string EndSessionEndpoint { get; set; }

        [JsonProperty("jwks_uri")]
        public string JwksUri { get; set; }
    }
}
