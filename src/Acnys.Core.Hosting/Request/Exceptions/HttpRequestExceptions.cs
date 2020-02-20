using System;
using System.Net;

namespace Acnys.Core.Hosting.Request.Exceptions
{
    public class HttpRequestException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public byte[] Content { get; }

        public HttpRequestException(HttpStatusCode statusCode, byte[] content)
        {
            Content = content;
            StatusCode = statusCode;
        }
    }
}
