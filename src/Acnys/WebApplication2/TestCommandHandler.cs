using Acnys.Core.Attributes;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;
using OpenTracing;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Commands;

namespace WebApplication2
{
    [HumanReadableInformation("Test command's handler", "Test command handler's deteailed description")]
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
            
            //var testEvent = new TestEvent(command.Data);
            //var args = new Dictionary<string, object>()
            //{
            //    { "test", "test" },
            //    { "int", 1 },
            //    { "Routing", "test.level" },
            //}.EnrichWithCorrelation(command, arguments);
            //var testEvent2 = new TestEvent(command.Data);

            //var args2 = new Dictionary<string, object>()
            //{
            //    { "test", "test2" },
            //    { "int", 2 },
            //    { "RoutingKey", "test.level" },
            //}.EnrichWithCorrelation(command, arguments);

            //await _eventPublisher.Publish(testEvent, args, cancellationToken);
            //await _eventPublisher.Publish(testEvent2, args2, cancellationToken);
        }
    }
}