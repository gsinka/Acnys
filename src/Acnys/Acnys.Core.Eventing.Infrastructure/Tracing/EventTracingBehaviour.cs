using Acnys.Core.Attributes;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Autofac.Features.Decorators;
using OpenTracing;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Eventing.Infrastructure.Tracing
{
    public class EventTracingBehaviour<TEvent> : IHandleEvent<TEvent> where TEvent : IEvent
    {
        private readonly IHandleEvent<TEvent> _nextEvent;
        private readonly ITracer _tracer;
        private readonly IDecoratorContext _decoratorContext;

        public EventTracingBehaviour(IHandleEvent<TEvent> nextCommand, ITracer tracer, IDecoratorContext decoratorContext)
        {
            _nextEvent = nextCommand;
            _tracer = tracer;
            _decoratorContext = decoratorContext;
        }
        public async Task Handle(TEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            if (@event.GetType().GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any() || _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any())
            {
                await _nextEvent.Handle(@event, arguments, cancellationToken);
                return;
            }
            var triggerInfo = @event.GetType().GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();
            var handlerInfo = _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();

            var span2 = TracingExtensions.StartNewSpanForHandler(_tracer, _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, @event.GetType().Namespace, @event.GetType().Name, handlerInfo, triggerInfo, arguments);

            Log.Verbose("Event span started");
            await _nextEvent.Handle(@event, arguments, cancellationToken);
            span2.Finish();
            Log.Verbose("Event span ended");
        }
    }
}
