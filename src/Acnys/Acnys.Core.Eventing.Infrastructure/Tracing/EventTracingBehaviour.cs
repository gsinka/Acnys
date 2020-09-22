using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Extensions;
using Microsoft.Extensions.Primitives;
using OpenTracing;
using OpenTracing.Propagation;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Eventing.Infrastructure.Tracing
{
    public class EventTracingBehaviour<TEvent> : IHandleEvent<TEvent> where TEvent : IEvent
    {
        private readonly ILogger _log;
        private readonly IHandleEvent<TEvent> _nextEvent;
        private readonly ITracer _tracer;

        public EventTracingBehaviour(ILogger log, IHandleEvent<TEvent> nextCommand, ITracer tracer)
        {
            _log = log;
            _nextEvent = nextCommand;
            _tracer = tracer;
        }
        public async Task Handle(TEvent command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            arguments ??= new Dictionary<string, object>();
            var headerExtractor = new TextMapExtractAdapter(arguments.ToDictionary(e => e.Key, v => v.Value.ToString()));
            var previousSpan = _tracer.Extract(BuiltinFormats.HttpHeaders, headerExtractor);
            ISpanBuilder spanbuilder;
            if (previousSpan != null)
                spanbuilder = _tracer.BuildSpan(typeof(TEvent).FullName).AddReference(References.ChildOf, previousSpan);
            else
            {
                spanbuilder = _tracer.BuildSpan(typeof(TEvent).FullName);
            }
            arguments.Remove("uber-trace-id");
            var traceExtendedHeaders = arguments.ToDictionary(e => e.Key, v => v.Value.ToString());
            var headerInjector = new TextMapInjectAdapter(traceExtendedHeaders);
            using var span = spanbuilder.StartActive(true);
            _tracer.Inject(span.Span.Context, BuiltinFormats.HttpHeaders, headerInjector);
            arguments.Add("uber-trace-id", new StringValues(traceExtendedHeaders["uber-trace-id"]).First());
            foreach (var arg in arguments)
            {
                span.Span.SetTag(new OpenTracing.Tag.StringTag(arg.Key), arg.Value.ToString());
            }
            Log.Verbose("Event span started");
            await _nextEvent.Handle(command, arguments, cancellationToken);
            Log.Verbose("Event span ended");
        }
    }
}
