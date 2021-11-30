using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DFC.Composite.Shell.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HttpResponseExtensions
    {
        public static string RetrieveCookieValue(this HttpResponse httpResponse, string name)
        {
            var cookieHeaders = httpResponse.GetTypedHeaders()?.SetCookie;
            return cookieHeaders?.FirstOrDefault(c => c.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase))?.Value.Value;
        }
    }
}
