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

            await using var scope = _scope.BeginLifetimeScope();

            try
            {
                _log.Verbose("Lifetime scope {scopeId} created for event", scope.GetHashCode());
                    
                var handlers = (IEnumerable<dynamic>)scope.Resolve(typeof(IEnumerable<>).MakeGenericType(handlerType));
                var allHandlers = scope.Resolve<IEnumerable<IHandleEvent>>();

                foreach (var handler in handlers.Union(allHandlers))
                {
                    _log.Debug("Handling event {eventName} with handler {handlerName}", eventName, (string)handler.GetType().Name);
                    await handler.Handle((dynamic)@event, arguments, cancellationToken);
                    _log.Debug("Finished handling event {eventName} with handler {handlerName}", eventName, (string)handler.GetType().Name);
                }

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
            finally
            {
                _log.Debug($"Ending lifetime scope: {scope.GetHashCode()}");
            }
        }
    }
}
