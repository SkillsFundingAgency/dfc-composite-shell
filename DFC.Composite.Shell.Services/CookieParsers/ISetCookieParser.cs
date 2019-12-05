namespace DFC.Composite.Shell.Services.CookieParsers
{
    /// <summary>
    /// Parses a set cookie value into a cookie settings object.
    /// </summary>
    public interface ISetCookieParser
    {
        CookieSettings Parse(string value);
    }
}
