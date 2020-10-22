using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using WebApplication1.Commands;

namespace WebApplication1
{
    public class TestEventHandler : IHandleEvent<TestEvent>
    {
        private readonly ISendRequest _requestSender;
        private readonly ISendCommand _commandSender;

        public TestEventHandler(ISendRequest requestSender)
        {
            _requestSender = requestSender;
        }

        public async Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            await _requestSender.Send(new AnotherTestCommand(""), new Dictionary<string, object>(), cancellationToken);
        }
    }
}