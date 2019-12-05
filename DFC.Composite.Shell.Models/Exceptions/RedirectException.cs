using System;
using System.Runtime.Serialization;

namespace DFC.Composite.Shell.Models.Exceptions
{
    [Serializable]
    public class RedirectException : Exception
    {
        [NonSerialized]
        private readonly Uri location;

        public RedirectException()
        {
        }

        protected RedirectException(SerializationInfo info, StreamingContext context)
        {
        }

        public RedirectException(Uri oldLocation, Uri location, bool isPermenant)
        {
            IsPermenant = isPermenant;
            this.OldLocation = oldLocation;
            this.location = location;
        }

        public Uri OldLocation { get; }

        public Uri Location => location;

        public bool IsPermenant { get; }
    }
}