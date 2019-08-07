using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.Services.CookieParsers
{
    /// <summary>
    /// Parses a cookie value into a cookie options object
    /// </summary>
    public interface ICookieParser
    {
        CookieOptions Parse(string value);
    }
}
