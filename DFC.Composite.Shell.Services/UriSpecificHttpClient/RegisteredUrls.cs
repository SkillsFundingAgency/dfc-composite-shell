using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.UriSpecificHttpClient
{
    public class RegisteredUrls : IRegisteredUrls
    {
        private readonly IEnumerable<RegisteredUrlModel> registeredUrls;

        public RegisteredUrls(IEnumerable<RegisteredUrlModel> registeredUrls)
        {
            this.registeredUrls = registeredUrls;
        }

        public IEnumerable<RegisteredUrlModel> GetAll()
        {
            return registeredUrls;
        }
    }
}