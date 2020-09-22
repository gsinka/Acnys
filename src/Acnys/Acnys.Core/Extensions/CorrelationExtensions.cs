using System;
using System.Collections.Generic;
using Acnys.Core.ValueObjects;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Acnys.Core.Extensions
{
    public static class CorrelationExtensions
    {
        public static Guid? CorrelationId(this IDictionary<string, object> arguments)
        {
            if (arguments == null || !arguments.ContainsKey(RequestConstants.CorrelationId)) return null;

            var value = arguments[RequestConstants.CorrelationId];

            return
                value is Guid guid
                    ? guid
                    : Guid.TryParse(value.ToString(), out Guid parsedGuid)
                        ? parsedGuid
                        : (Guid?)null;
        }
        public static string TraceId(this IDictionary<string, object> arguments)
        {
            if (arguments == null || !arguments.ContainsKey(RequestConstants.TraceId)) return null;

            var value = arguments[RequestConstants.TraceId];
            
            return value.ToString();
        }

        public static Guid? CausationId(this IDictionary<string, object> arguments)
        {
            if (arguments == null || !arguments.ContainsKey(RequestConstants.CausationId)) return null;

            var value = arguments[RequestConstants.CausationId];

            return
                value is Guid guid
                    ? guid
                    : Guid.TryParse(value.ToString(), out Guid parsedGuid)
                        ? parsedGuid
                        : (Guid?)null;
        }

        public static IDictionary<string, object> UseCorrelationId(this IDictionary<string, object> arguments, Guid? correlationId)
        {
            if (!correlationId.HasValue) return arguments;
            arguments.Add(RequestConstants.CorrelationId, correlationId);
            return arguments;
        }
        public static IDictionary<string, object> UseTraceId(this IDictionary<string, object> arguments, string traceId)
        {
            if (string.IsNullOrWhiteSpace(traceId)) return arguments;
            arguments.Add(RequestConstants.TraceId, traceId);
            return arguments;
        }
        public static IDictionary<string, object> UseCausationId(this IDictionary<string, object> arguments, Guid? causationId)
        {
            if (!causationId.HasValue) return arguments;
            arguments.Add(RequestConstants.CausationId, causationId);
            return arguments;
        }

        public static void EnrichLogContextWithCorrelation(this IDictionary<string, object> args)
        {
            var correlationId = args?.CorrelationId();
            if (correlationId != null) LogContext.PushProperty(RequestConstants.CorrelationId, correlationId);

            var causationId = args?.CausationId();
            if (causationId != null) LogContext.PushProperty(RequestConstants.CausationId, causationId);
        }
    }
}
