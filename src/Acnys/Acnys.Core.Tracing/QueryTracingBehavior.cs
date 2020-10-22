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
    public class QueryTracingBehavior : IDispatchQuery
    {
        private readonly ILogger _log;
        private readonly ITracer _tracer;
        private readonly TracingContext _tracingContext;
        private readonly IDispatchQuery _next;
        private readonly IDecoratorContext _decoratorContext;

        public QueryTracingBehavior(ILogger log, ITracer tracer, TracingContext tracingContext, IDispatchQuery next, IDecoratorContext decoratorContext)
        {
            _log = log;
            _tracer = tracer;
            _tracingContext = tracingContext;
            _next = next;
            _decoratorContext = decoratorContext;
        }

        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            if (query.GetType().GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any() || _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<ExcludeFromTracingAttribute>().Any())
            {
                return await _next.Dispatch(query, arguments, cancellationToken);
            }
            var triggerInfo = query.GetType().GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();
            var handlerInfo = _decoratorContext.ImplementationType.GetCustomAttributes(false).OfType<HumanReadableInformationAttribute>().FirstOrDefault();

            var span2 = TracingExtensions.StartNewSpanForHandler(_tracer, _decoratorContext.ImplementationType.Namespace, _decoratorContext.ImplementationType.Name, query.GetType().Namespace, query.GetType().Name, handlerInfo, triggerInfo, arguments);

            _tracingContext.Update(_log, query, arguments);

            Log.Verbose("Event span started");
            var response = await _next.Dispatch(query, arguments, cancellationToken);
            span2.Finish();
            Log.Verbose("Event span ended");

            return response;
        }
    }
}