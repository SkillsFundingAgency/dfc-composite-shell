using System;

namespace DFC.Composite.Shell.Services.Auth.Models
{
    public class AuthSettings
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string ClientSecret { get; set; }

        public Uri DefaultRedirectUrl { get; set; }

        public string AuthDssEndpoint { get; set; }
    }
}
