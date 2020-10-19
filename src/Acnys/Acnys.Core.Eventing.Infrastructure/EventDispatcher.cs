using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Autofac;
using Serilog;

namespace Acnys.Core.Eventing.Infrastructure
{
    public class EventDispatcher : IDispatchEvent
    {
        private readonly ILogger _log;
        private readonly ILifetimeScope _scope;
        private readonly EventAwaiterService _eventAwaiterService;

        public EventDispatcher(ILogger log, ILifetimeScope scope, EventAwaiterService eventAwaiterService = null)
        {
            _log = log;
            _scope = scope;
            _eventAwaiterService = eventAwaiterService;
        }

        public async Task Dispatch<T>(T @event, IDictionary<string, object> arguments, CancellationToken cancellationToken = default) where T : IEvent
        {
            var eventName = @event.GetType().Name;

            _log.Debug("Dispatching event {eventName}", eventName);

            _log.Verbose("Event data: {@event}", @event);
            _log.Verbose("Event arguments: {@arguments}", arguments);

            var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());

            try
            {
                //await using var scope = _scope.BeginLifetimeScope();

                var handlers = (IEnumerable<dynamic>)_scope.Resolve(typeof(IEnumerable<>).MakeGenericType(handlerType));
                var allHandlers = _scope.Resolve<IEnumerable<IHandleEvent>>();

                foreach (var handler in handlers.Union(allHandlers))
                {
                    _log.Debug("Handling event {eventName} with handler {handlerName} ({handlerId})", eventName, (string)handler.GetType().Name, handler.GetHashCode());
                    await handler.Handle((dynamic)@event, arguments, cancellationToken);
                    _log.Debug("Finished handling event {eventName} with handler {handlerName} ({handlerId})", eventName, (string)handler.GetType().Name, handler.GetHashCode());
                }

                //TODO: obsolate, remove in version 1.0

                if (_eventAwaiterService != null)
                {
                    await _eventAwaiterService.ProcessEvent(@event, arguments, cancellationToken);
                }
                
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Event handling failed: {error} ", exception.Message);
                throw;
            }
        }
    }
}
