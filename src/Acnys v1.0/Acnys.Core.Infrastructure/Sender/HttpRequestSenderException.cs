using System;
using System.Net;
using System.Runtime.Serialization;

namespace Acnys.Core.Infrastructure.Sender
{
    public class HttpRequestSenderException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpRequestSenderException()
        {
        }

        protected HttpRequestSenderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HttpRequestSenderException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpRequestSenderException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}