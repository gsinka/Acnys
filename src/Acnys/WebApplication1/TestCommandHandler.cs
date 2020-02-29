using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;
using Serilog;

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

            await _eventPublisher.Publish(testEvent, args, cancellationToken);
        }
    }
}