using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Composite.Shell.Services.UriSpecificHttpClient
{
    [ExcludeFromCodeCoverage]
    public class RegisteredUrlEquals : IEqualityComparer<RegisteredUrlModel>
    {
        public bool Equals(RegisteredUrlModel x, RegisteredUrlModel y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Url == y.Url && x.IsInteractiveApp == y.IsInteractiveApp;
        }

        public int GetHashCode(RegisteredUrlModel obj)
        {
            return HashCode.Combine(obj.Url, obj.IsInteractiveApp);
        }
    }
}
