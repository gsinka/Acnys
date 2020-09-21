using Acnys.Core.Eventing.Abstractions;
using OpenTracing;
using Serilog;
using System.Collections.Generic;
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
            var spanbuilder = _tracer.BuildSpan(typeof(TEvent).Name);
            using var span = spanbuilder.StartActive(true);
            
            foreach (var arg in arguments)
            {
                span.Span.SetBaggageItem(arg.Key, arg.Value.ToString());
                span.Span.SetTag(new OpenTracing.Tag.StringTag(arg.Key), arg.Value.ToString());
            }
            Log.Verbose("Event span started");
            await _nextEvent.Handle(command, arguments, cancellationToken);
            Log.Verbose("Event span ended");
        }
    }
}
