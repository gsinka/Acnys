using Microsoft.AspNetCore.Http;
using System;

namespace Acnys.Core.AspNet.Request
{
    public static class HttpRequestExtensions
    {
        public static Guid GetCorrelationId(this HttpRequest request)
        {
            return request.Headers.TryGetValue("correlationId", out var correlationIdString) ? Guid.Parse(correlationIdString) : Guid.NewGuid();
        }
    }
}
