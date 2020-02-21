using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;

namespace WebApplication1
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        private readonly IPublishEvent _eventPublisher;

        public TestCommandHandler(IPublishEvent eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            var testEvent = new TestEvent(command.Data);
            var args = new Dictionary<string, object>() { { "test", "test" }, { "int", 1 } }.EnrichWithCorrelation(command, arguments);

            await _eventPublisher.Publish(testEvent, args, cancellationToken);
        }
    }
}