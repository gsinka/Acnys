using Acnys.Core.Attributes;
using Acnys.Core.Extensions;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Services;
using Autofac.Features.Decorators;
using OpenTracing;
using Prometheus;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Infrastructure.Tracing
{
    public class RequestTracingBehaviour<TQuery, TResult> : IHandleQuery<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        private readonly IHandleQuery<TQuery, TResult> _nextCommand;
        private readonly MetricsService _metricsService;
        private readonly IDecoratorContext _decoratorContext;
        private readonly ITracer _tracer;

        public RequestTracingBehaviour(IHandleQuery<TQuery, TResult> nextCommand, MetricsService metricsService, IDecoratorContext decoratorContext, ITracer tracer)
        {
            _nextCommand = nextCommand;
            _metricsService = metricsService;
            _decoratorContext = decoratorContext;
            _tracer = tracer;
        }

        public async Task<TResult> Handle(TQuery command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {

            var triggerInfo = command.GetType().GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();
            var handlerInfo = _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();

            var span2 = TracingExtensions.StartNewSpanForHandler(_tracer, _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, command.GetType().Namespace, command.GetType().Name, handlerInfo, triggerInfo, arguments);

            Log.Verbose("Event span started");
            var response = await _nextCommand.Handle(command, arguments, cancellationToken);
            span2.Finish();
            Log.Verbose("Event span ended");

            return response;
        }
    }
}
