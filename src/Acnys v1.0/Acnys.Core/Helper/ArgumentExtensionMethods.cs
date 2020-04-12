using System;
using System.Collections.Generic;
using Acnys.Core.Abstractions;
using Acnys.Core.ValueObjects;

namespace Acnys.Core.Helper
{
    public static class ArgumentExtensionMethods
    {
        public static Guid? CorrelationId(this IDictionary<string, object> arguments)
        {
            if (arguments == null || !arguments.ContainsKey(RequestConstants.CorrelationId)) return null;

            var value = arguments[RequestConstants.CorrelationId];

            return value is Guid guid
                    ? guid
                    : Guid.TryParse(value.ToString(), out var parsedGuid)
                        ? parsedGuid
                        : (Guid?)null;
        }

        public static Guid? CausationId(this IDictionary<string, object> arguments)
        {
            if (arguments == null || !arguments.ContainsKey(RequestConstants.CausationId)) return null;

            var value = arguments[RequestConstants.CausationId];

            return value is Guid guid
                    ? guid
                    : Guid.TryParse(value.ToString(), out var parsedGuid)
                        ? parsedGuid
                        : (Guid?)null;
        }

        public static IDictionary<string, object> UseCorrelationId(this IDictionary<string, object> arguments, Guid? correlationId)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments), "Argument");
            if (!correlationId.HasValue) return arguments;
            arguments?.Add(RequestConstants.CorrelationId, correlationId);
            return arguments;
        }

        public static IDictionary<string, object> UseCausationId(this IDictionary<string, object> arguments, Guid? causationId)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments), "Argument");
            if (!causationId.HasValue) return arguments;
            arguments.Add(RequestConstants.CausationId, causationId);
            return arguments;
        }

        public static IDictionary<string, object> CausedBy(this IDictionary<string, object> arguments, IEvent @event)
        {
            arguments.UseCausationId(@event.EventId);
            return arguments;
        }

        public static IDictionary<string, object> CausedBy(this IDictionary<string, object> arguments, ICommand command)
        {
            arguments.UseCausationId(command.RequestId);
            return arguments;
        }
    }
}
