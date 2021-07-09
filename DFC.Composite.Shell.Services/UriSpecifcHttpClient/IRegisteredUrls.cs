using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.UriSpecifcHttpClient
{
    public interface IRegisteredUrls
    {
        public IEnumerable<string> GetAll();
    }
}
