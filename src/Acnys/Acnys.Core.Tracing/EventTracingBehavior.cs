using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Tracing.Attributes;
using Autofac.Features.Decorators;
using OpenTracing;
using Serilog;

namespace Acnys.Core.Tracing
{
    public class EventTracingBehavior : IDispatchEvent
    {
        private readonly ILogger _log;
        private readonly ITracer _tracer;
        private readonly TracingContext _tracingContext;
        private readonly IDispatchEvent _next;
        private readonly IDecoratorContext _decoratorContext;

        public EventTracingBehavior(ILogger log, ITracer tracer, TracingContext tracingContext, IDispatchEvent next, IDecoratorContext decoratorContext)
        {
            _log = log;
            _tracer = tracer;
            _tracingContext = tracingContext;
            _next = next;
            _decoratorContext = decoratorContext;
        }

        public async Task Dispatch<T>(T @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            if (@event.GetType().GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any() || _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any())
            {
                await _next.Dispatch(@event, arguments, cancellationToken);
                return;
            }
            var triggerInfo = @event.GetType().GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();
            var handlerInfo = _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();

            var span2 = TracingExtensions.StartNewSpanForHandler(_tracer, _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, @event.GetType().Namespace, @event.GetType().Name, handlerInfo, triggerInfo, arguments);

            _tracingContext.Update(_log, @event, arguments);

            Log.Verbose("Event span started");
            await _next.Dispatch(@event, arguments, cancellationToken);
            span2.Finish();
            Log.Verbose("Event span ended");



        }
    }
}