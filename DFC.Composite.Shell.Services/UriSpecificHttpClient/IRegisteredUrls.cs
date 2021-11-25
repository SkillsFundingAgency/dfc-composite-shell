using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.UriSpecificHttpClient
{
    public interface IRegisteredUrls
    {
        public IEnumerable<RegisteredUrlModel> GetAll();
    }
}