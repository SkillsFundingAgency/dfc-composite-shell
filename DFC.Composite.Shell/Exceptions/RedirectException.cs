using System;

namespace DFC.Composite.Shell.Exceptions
{
    public class RedirectException : Exception
    {
        private readonly Uri location;

        public RedirectException(Uri oldLocation, Uri location, bool isPermenant)
        {
            IsPermenant = isPermenant;
            this.OldLocation = oldLocation;
            this.location = location;
        }

        public RedirectException()
        {
        }

        public RedirectException(string message) : base(message)
        {
        }

        public RedirectException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public Uri OldLocation { get; }

        public Uri Location => location;

        public bool IsPermenant { get; }
    }
}