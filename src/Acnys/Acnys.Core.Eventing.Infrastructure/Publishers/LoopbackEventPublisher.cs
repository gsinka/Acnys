using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions.Extensions;
using Acnys.Core.Eventing.Abstractions;
using Serilog;
using Serilog.Context;

namespace Acnys.Core.Eventing.Infrastructure.Publishers
{
    public class LoopbackEventPublisher : IPublishEvent
    {
        private readonly IDispatchEvent _eventDispatcher;
        private readonly ILogger _log;

        public LoopbackEventPublisher(IDispatchEvent eventDispatcher, ILogger log)
        {
            _eventDispatcher = eventDispatcher;
            _log = log;
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments, CancellationToken cancellationToken = default) where T : IEvent
        {
            using var correlationId = LogContext.PushProperty("correlationId", arguments.CorrelationId());
            using var causationId = LogContext.PushProperty("causationId", arguments.CausationId());

            await _eventDispatcher.Dispatch(@event, arguments, cancellationToken);
        }
    }
}
