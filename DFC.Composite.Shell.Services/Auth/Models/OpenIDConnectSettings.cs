namespace DFC.Composite.Shell.Services.Auth.Models
{
    public class OpenIDConnectSettings
    {
        public string OIDCConfigMetaDataUrl { get; set; }

        public bool UseOIDCConfigDiscovery { get; set; }

        public string ClientId { get; set; }

        public string AuthorizeUrl { get; set; }

        public string JWKsUrl { get; set; }

        public string JWK { get; set; }

        public string Issuer { get; set; }

        public string RedirectUrl { get; set; }

        public string AuthdUrl { get; set; }

        public bool LogPersonalInfo { get; set; }

        public string EndSessionUrl { get; set; }

        public string SignOutRedirectUrl { get; set; }

        public string Exponent { get; set; }
    }
}
