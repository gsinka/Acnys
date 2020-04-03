using Microsoft.AspNetCore.Http;
using System;
using Acnys.Core.ValueObjects;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.AspNet.Request
{
    public static class HttpRequestExtensions
    {
        public static Guid? CorrelationId(this HttpRequest request) =>
            request.Headers.TryGetValue(RequestConstants.CorrelationId, out var correlationIdString) ? Guid.Parse(correlationIdString) : (Guid?) null;

        public static Guid? CausationId(this HttpRequest request) =>
            request.Headers.TryGetValue(RequestConstants.CausationId, out var causationIdString) ? Guid.Parse(causationIdString) : (Guid?)null;

        public static Guid? RequestId(this HttpRequest request) =>
            request.Headers.TryGetValue(RequestConstants.RequestId, out var requestIdString) ? Guid.Parse(requestIdString) : (Guid?)null;

        public static Type DomainType(this HttpRequest request) =>
            request.Headers.TryGetValue(RequestConstants.DomainType, out var domainTypeString) ? Type.GetType(domainTypeString) : null;

        public static void EnrichWithHttpRequest(this ILogger log, HttpRequest request)
        {
            LogContext.PushProperty(RequestConstants.CorrelationId, request.CorrelationId());
            LogContext.PushProperty(RequestConstants.CausationId, request.CausationId());
            LogContext.PushProperty(RequestConstants.RequestId, request.RequestId());
        }
    }
}
