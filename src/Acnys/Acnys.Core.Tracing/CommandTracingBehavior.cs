using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Tracing.Attributes;
using Autofac.Features.Decorators;
using OpenTracing;
using Serilog;

namespace Acnys.Core.Tracing
{
    public class CommandTracingBehavior : IDispatchCommand
    {
        private readonly ILogger _log;
        private readonly ITracer _tracer;
        private readonly TracingContext _tracingContext;
        private readonly IDispatchCommand _next;
        private readonly IDecoratorContext _decoratorContext;

        public CommandTracingBehavior(ILogger log, ITracer tracer, TracingContext tracingContext, IDispatchCommand next, IDecoratorContext decoratorContext)
        {
            _log = log;
            _tracer = tracer;
            _tracingContext = tracingContext;
            _next = next;
            _decoratorContext = decoratorContext;
        }

        public async Task Dispatch<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            if (command.GetType().GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any() || _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any())
            {
                await _next.Dispatch(command, arguments, cancellationToken);
                return;
            }
            var triggerInfo = command.GetType().GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();
            var handlerInfo = _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();

            var span2 = TracingExtensions.StartNewSpanForHandler(_tracer, _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, command.GetType().Namespace, command.GetType().Name, handlerInfo, triggerInfo, arguments);

            _tracingContext.Update(_log, command, arguments);

            Log.Verbose("CommandEvent span started");
            await _next.Dispatch(command, arguments, cancellationToken);
            span2.Finish();
            Log.Verbose("Command span ended");

        }
    }
}
