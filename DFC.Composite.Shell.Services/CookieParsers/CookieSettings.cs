using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.Services.CookieParsers
{
    public class CookieSettings
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public CookieOptions CookieOptions { get; set; } = new CookieOptions();
    }
}
