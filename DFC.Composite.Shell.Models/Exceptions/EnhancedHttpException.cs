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

        public EnhancedHttpException(SerializationInfo info, StreamingContext context)
        {
        }

        public EnhancedHttpException(string message) : base(message)
        {
        }

        public EnhancedHttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EnhancedHttpException(HttpStatusCode statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }

        public EnhancedHttpException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
