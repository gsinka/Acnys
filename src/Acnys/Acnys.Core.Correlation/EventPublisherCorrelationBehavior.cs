using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Serilog;

namespace Acnys.Core.Correlation
{
    public class EventPublisherCorrelationBehavior : IPublishEvent
    {
        private readonly ILogger _log;
        private readonly CorrelationContext _correlationContext;
        private readonly IPublishEvent _next;

        public EventPublisherCorrelationBehavior(ILogger log, CorrelationContext correlationContext, IPublishEvent next)
        {
            _log = log;
            _correlationContext = correlationContext;
            _next = next;
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.UseCorrelationId(_correlationContext.CorrelationId);
            arguments.UseCausationId(_correlationContext.CausationId);
            _log.Debug("Updating arguments from Correlation context {contextId}: {@context}", _correlationContext.GetHashCode(), _correlationContext);

            await _next.Publish(@event, arguments, cancellationToken);
        }

        public async Task Publish<T>(T @event, string routingKey, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.UseCorrelationId(_correlationContext.CorrelationId);
            arguments.UseCausationId(_correlationContext.CausationId);
            _log.Debug("Updating arguments from Correlation context {contextId}: {@context}", _correlationContext.GetHashCode(), _correlationContext);

            await _next.Publish(@event, routingKey, arguments, cancellationToken);
        }
    }
}