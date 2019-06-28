using System;

namespace DFC.Composite.Shell.Exceptions
{
    public class RedirectException : Exception
    {
        private readonly Uri _oldLocation;
        private readonly Uri _location;

        public Uri OldLocation => _oldLocation;
        public Uri Location => _location;

        public RedirectException(Uri oldLocation, Uri location)
        {
            _oldLocation = oldLocation;
            _location = location;
        }
    }
}
