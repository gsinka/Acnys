using System;
using System.Collections.Generic;

namespace Acnys.Core.Abstractions.Extensions
{
    public static class CorrelationExtensions
    {
        public const string CorrelationIdName = "correlation-id";
        public const string CausationIdName = "causation-id";

        public static Guid? CorrelationId(this IDictionary<string, object> arguments)
        {
            return (arguments != null && arguments.ContainsKey(CorrelationIdName) ? arguments[CorrelationIdName] as Guid? : null);
        }

        public static Guid? CausationId(this IDictionary<string, object> arguments)
        {
            return (arguments != null && arguments.ContainsKey(CausationIdName) ? arguments[CausationIdName] as Guid? : null);
        }

        public static IDictionary<string, object> UseCorrelationId(this IDictionary<string, object> arguments, Guid correlationId)
        {
            arguments.Add(CorrelationIdName, correlationId);
            return arguments;
        }
        
        public static IDictionary<string, object> UseCausationId(this IDictionary<string, object> arguments, Guid causationId)
        {
            arguments.Add(CausationIdName, causationId);
            return arguments;
        }
    }
}
