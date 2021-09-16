using System.Collections.Generic;

namespace DFC.Composite.Shell.Services.UriSpecifcHttpClient
{
    public class RegisteredUrls : IRegisteredUrls
    {
        private readonly IEnumerable<string> registeredUrls;

        public RegisteredUrls(IEnumerable<string> registeredUrls)
        {
            this.registeredUrls = registeredUrls;
        }

        public IEnumerable<string> GetAll()
        {
            return registeredUrls;
        }
    }
}