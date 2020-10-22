using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Serilog;

namespace Acnys.Core.Tracing
{
    public class EventPublisherTracingBehavior : IPublishEvent
    {
        private readonly ILogger _log;
        private readonly TracingContext _tracingContext;
        private readonly IPublishEvent _next;

        public EventPublisherTracingBehavior(ILogger log, TracingContext tracingContext, IPublishEvent next)
        {
            _log = log;
            _tracingContext = tracingContext;
            _next = next;
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.UpdateWithTracingContext(_log, _tracingContext);
            await _next.Publish(@event, arguments, cancellationToken);
        }

        public async Task Publish<T>(T @event, string routingKey, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.UpdateWithTracingContext(_log, _tracingContext);
            await _next.Publish(@event, routingKey, arguments, cancellationToken);
        }
    }
}