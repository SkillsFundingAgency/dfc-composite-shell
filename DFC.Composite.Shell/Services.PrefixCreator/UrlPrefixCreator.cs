using System;
using System.Linq;

namespace DFC.Composite.Shell.Services.PrefixCreator
{
    /// <summary>
    /// Creates prefixes based on a url path
    /// </summary>
    public class UrlPrefixCreator : IPrefixCreator
    {
        public string Resolve(Uri uri)
        {
            var result = uri.Segments.ElementAtOrDefault(1);
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Replace("/", string.Empty);
            }
            return result;
        }
    }
}
