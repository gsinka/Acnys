using System;
using System.Collections.Generic;
using System.Linq;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.ValueObjects;
using Autofac;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.Correlation
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
            if (!arguments.ContainsKey(RequestConstants.CorrelationId))
            {
                arguments.Add(RequestConstants.CorrelationId, correlationId);
            }
            else
            {
                arguments[RequestConstants.CorrelationId] = correlationId;
            }

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
            if (!arguments.ContainsKey(RequestConstants.CausationId))
            {
                arguments.Add(RequestConstants.CausationId, causationId);
            }
            else
            {
                arguments[RequestConstants.CausationId] = causationId;
            }
            return arguments;
        }

        public static void EnrichLogContextWithCorrelation(this IDictionary<string, object> args)
        {
            var correlationId = args?.CorrelationId();
            if (correlationId != null) LogContext.PushProperty(RequestConstants.CorrelationId, correlationId);

            var causationId = args?.CausationId();
            if (causationId != null) LogContext.PushProperty(RequestConstants.CausationId, causationId);
        }

        public static IDictionary<string, object> UpdateWithCorrelationContext(this IDictionary<string, object> arguments, ILogger log, CorrelationContext correlationContext)
        {
            arguments.UseCorrelationId(correlationContext.CorrelationId);
            arguments.UseCausationId(correlationContext.CausationId);
            log.Debug("Updating arguments from Correlation context {contextId}: {@context}", correlationContext.GetHashCode(), correlationContext);

            return arguments;
        }

        public static IDictionary<string, object> UpdateCausationPath(this IDictionary<string, object> arguments, IRequest request)
        {
            if (!arguments.ContainsKey(RequestConstants.CausationPath))
            {
                arguments.Add(RequestConstants.CausationPath, $"{request.GetType().FullName} ({request.RequestId})");
            }
            else
            {
                arguments[RequestConstants.CausationPath] = $"{arguments[RequestConstants.CausationPath]} -> {request.GetType().FullName} ({request.RequestId})";
            }

            return arguments;
        }

        public static IDictionary<string, object> UpdateCausationPath(this IDictionary<string, object> arguments, IEvent evnt)
        {
            if (!arguments.ContainsKey(RequestConstants.CausationPath))
            {
                arguments.Add(RequestConstants.CausationPath, $"{evnt.GetType().FullName} ({evnt.EventId})");
            }
            else
            {
                arguments[RequestConstants.CausationPath] = $"{arguments[RequestConstants.CausationPath]} -> {evnt.GetType().FullName} ({evnt.EventId})";
            }

            return arguments;
        }

        public static CorrelationContext Update(this CorrelationContext correlationContext, ILogger log, IEvent evnt, IDictionary<string, object> arguments)
        {
            correlationContext.CorrelationId = arguments?.CorrelationId() ?? Guid.NewGuid();
            correlationContext.CausationId = evnt.EventId;
            //if (arguments != null && arguments.ContainsKey(RequestConstants.CausationPath)) correlationContext.CausationPath = arguments[RequestConstants.CausationPath] as string;

            log.Debug("Correlation context {contextId} updated: {@context}", correlationContext.GetHashCode(), correlationContext);

            return correlationContext;
        }

        public static CorrelationContext Update(this CorrelationContext correlationContext, ILogger log, IRequest request, IDictionary<string, object> arguments)
        {
            correlationContext.CorrelationId = arguments?.CorrelationId() ?? Guid.NewGuid();
            correlationContext.CausationId = request.RequestId;
            //if (arguments != null && arguments.ContainsKey(RequestConstants.CausationPath)) correlationContext.CausationPath = arguments[RequestConstants.CausationPath] as string;

            log.Debug("Correlation context {contextId} updated: {@context}", correlationContext.GetHashCode(), correlationContext);

            return correlationContext;
        }
    }
}
