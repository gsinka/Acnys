using System.Collections.Generic;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.Infrastructure
{
    public static class ArgumentExtensions
    {
        public static void EnrichLog(this IDictionary<string, object> arguments)
        {
            if (arguments == null) return;

            foreach (var argument in arguments)
            {
                LogContext.PushProperty(argument.Key, argument.Value);
            }
        }
    }
}