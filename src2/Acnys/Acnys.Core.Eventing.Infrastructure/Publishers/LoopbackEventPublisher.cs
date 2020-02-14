using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;

namespace Acnys.Core.Eventing.Infrastructure.Publishers
{
    public class LoopbackEventPublisher : IPublishEvent
    {
        private readonly IDispatchEvent _eventDispatcher;

        public LoopbackEventPublisher(IDispatchEvent eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments, CancellationToken cancellationToken = default) where T : IEvent
        {
            await _eventDispatcher.Dispatch(@event, arguments, cancellationToken);
        }
    }
}
