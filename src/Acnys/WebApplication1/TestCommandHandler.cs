using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;
using Serilog;
using WebApplication1.Commands;

namespace WebApplication1
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        private readonly IPublishEvent _eventPublisher;
        private readonly ILogger _log;
        private readonly UserContext _context;

        public TestCommandHandler(IPublishEvent eventPublisher, ILogger log, UserContext context)
        {
            _eventPublisher = eventPublisher;
            _log = log;
            _context = context;
        }

        public async Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var testEvent = new TestEvent(command.Data);
            var args = new Dictionary<string, object>()
            {
                { "test", "test" }, 
                { "int", 1 },
                { "RoutingKey", "test.level" }
            }.EnrichWithCorrelation(command, arguments);

            var testEvent2= new TestEvent(command.Data);

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