using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            await Publish(@event, null, arguments, cancellationToken);
        }

        public async Task Publish<T>(T @event, string routingKey, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.EnrichLogContextWithCorrelation();

            await _eventDispatcher.Dispatch(@event, arguments, cancellationToken);
        }
    }
}
