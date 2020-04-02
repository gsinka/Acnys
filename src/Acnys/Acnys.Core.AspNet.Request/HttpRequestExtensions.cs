using Microsoft.AspNetCore.Http;
using System;

namespace Acnys.Core.AspNet.Request
{
    public static class HttpRequestExtensions
    {
        public static Guid? CorrelationId(this HttpRequest request) =>
            request.Headers.TryGetValue(HttpRequestHeaderConstants.CorrelationId, out var correlationIdString) ? Guid.Parse(correlationIdString) : (Guid?) null;

        public static Guid? CausationId(this HttpRequest request) =>
            request.Headers.TryGetValue(HttpRequestHeaderConstants.CausationId, out var causationIdString) ? Guid.Parse(causationIdString) : (Guid?)null;

        public static Type DomainType(this HttpRequest request) =>
            request.Headers.TryGetValue(HttpRequestHeaderConstants.DomainType, out var domainTypeString) ? Type.GetType(domainTypeString) : null;


    }
}
