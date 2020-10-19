using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Correlation
{
    public class EventCorrelationBehavior : IDispatchEvent
    {
        private readonly ILogger _log;
        private readonly CorrelationContext _correlationContext;
        private readonly IDispatchEvent _next;
        
        public EventCorrelationBehavior(ILogger log, CorrelationContext correlationContext, IDispatchEvent next)
        {
            _log = log;
            _correlationContext = correlationContext;
            _next = next;
        }

        public async Task Dispatch<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            _correlationContext.Update(_log, @event, arguments);
            await _next.Dispatch(@event, arguments, cancellationToken);
        }
    }
}