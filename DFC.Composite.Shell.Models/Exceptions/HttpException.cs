using System;
using System.Net;
using System.Runtime.Serialization;

namespace DFC.Composite.Shell.Models.Exceptions
{
    [Serializable]
    public class HttpException : Exception
    {
        public HttpException() {}

        protected HttpException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public HttpException(string message) : base(message) {}

        public HttpException(string message, Exception innerException) : base(message, innerException) {}

        public HttpException(HttpStatusCode statusCode, string message, string url) : base(message)
        {
            StatusCode = statusCode;
            Url = url;
        }

        public HttpException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }

        public string Url { get; }
    }
}
