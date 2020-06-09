using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Autofac;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Eventing.Infrastructure.Publishers
{
    public class LoopbackEventPublisher : IPublishEvent
    {
        private readonly ILogger _log;
        private readonly ILifetimeScope _scope;

        public LoopbackEventPublisher(ILogger log, ILifetimeScope scope)
        {
            _log = log;
            _scope = scope;
        }

        public async Task Publish<T>(T @event, IDictionary<string, object> arguments, CancellationToken cancellationToken = default) where T : IEvent
        {
            await Publish(@event, null, arguments, cancellationToken);
        }

        public async Task Publish<T>(T @event, string routingKey, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            arguments.EnrichLogContextWithCorrelation();

            await using var scope = _scope.BeginLifetimeScope();
            _log.Debug("New lifetime scope created for event dispatcher ({scopeId})", scope.GetHashCode());

            await scope.Resolve<IDispatchEvent>().Dispatch(@event, arguments, cancellationToken);

            _log.Debug("Ending lifetime scope ({scopeId})", scope.GetHashCode());
        }
    }
}
