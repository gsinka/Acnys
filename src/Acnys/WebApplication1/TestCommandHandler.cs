using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;
using Jaeger;
using OpenTracing;
using OpenTracing.Propagation;
using Serilog;
using WebApplication1.Commands;

namespace WebApplication1
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        private readonly IPublishEvent _eventPublisher;
        private readonly ILogger _log;
        private readonly UserContext _context;
        private readonly ITracer _tracer;

        public TestCommandHandler(IPublishEvent eventPublisher, ILogger log, UserContext context, ITracer tracer)
        {
            _eventPublisher = eventPublisher;
            _log = log;
            _context = context;
            _tracer = tracer;
        }

        public async Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            //var spanBuilder1 = _tracer.BuildSpan("operation 1");
            //using (var scope = spanBuilder1.StartActive(true))
            //{
            //    scope.Span.Log("ok");
            //    scope.Span.SetTag(new OpenTracing.Tag.StringTag("Name"), "A");
            //    var spanBuilder2 = _tracer.BuildSpan("operation 2");
            //    using (var scope2 = spanBuilder2.StartActive(true))
            //    {
            //        scope2.Span.Log("ok");
            //        scope2.Span.SetTag(new OpenTracing.Tag.StringTag("Name"), "B");
            //    }
            //}

            var testEvent = new TestEvent(command.Data);
            var args = new Dictionary<string, object>()
            {
                { "test", "test" },
                { "int", 1 },
                { "RoutingKey", "test.level" }
            }.EnrichWithCorrelation(command, arguments);

            var testEvent2 = new TestEvent(command.Data);

            var args2 = new Dictionary<string, object>()
            {
                { "test", "test2" },
                { "int", 2 },
                { "RoutingKey", "test.level" }
            }.EnrichWithCorrelation(command, arguments);

            await _eventPublisher.Publish(testEvent, args, cancellationToken);
            await _eventPublisher.Publish(testEvent2, args2, cancellationToken);
        }
    }
}