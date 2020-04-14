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
            return arguments.GetGuidFromArguments(RequestConstants.CorrelationId);
        }

        public static Guid? CausationId(this IDictionary<string, object> arguments)
        {
            return arguments.GetGuidFromArguments(RequestConstants.CausationId);
        }

        public static Guid? RequestId(this IDictionary<string, object> arguments)
        {
            return arguments.GetGuidFromArguments(RequestConstants.RequestId);
        }

        private static Guid? GetGuidFromArguments(this IDictionary<string, object> arguments, string key)
        {
            if (arguments == null || !arguments.ContainsKey(key)) return null;

            var value = arguments[key];

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
