using System;
using System.Runtime.Serialization;

namespace DFC.Composite.Shell.Models.Exceptions
{
    [Serializable]
    public class RedirectRequest : Exception
    {
        public RedirectRequest() {}

        protected RedirectRequest(SerializationInfo info, StreamingContext context) {}

        public RedirectRequest(Uri oldLocation, Uri location, bool isPermenant)
        {
            OldLocation = oldLocation;
            Location = location;
            IsPermenant = isPermenant;
        }

        public Uri OldLocation { get; }

        public Uri Location { get; }

        public bool IsPermenant { get; }
    }
}