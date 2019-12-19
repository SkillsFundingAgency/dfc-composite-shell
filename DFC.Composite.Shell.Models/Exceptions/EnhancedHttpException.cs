using System;
using System.Net;
using System.Runtime.Serialization;

namespace DFC.Composite.Shell.Models.Exceptions
{
    [Serializable]
    public class EnhancedHttpException : Exception
    {
        public EnhancedHttpException()
        {
        }

        protected EnhancedHttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EnhancedHttpException(string message) : base(message)
        {
        }

        public EnhancedHttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EnhancedHttpException(HttpStatusCode statusCode, string message, string url) : base(message)
        {
            this.StatusCode = statusCode;
            this.Url = url;
        }

        public EnhancedHttpException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }

        public string Url { get; }
    }
}
