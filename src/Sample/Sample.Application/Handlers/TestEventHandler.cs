using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Request.Application;
using Sample.Api.Requests;
using Serilog;

namespace Sample.Application.Handlers
{
    public class TestEventHandler : IHandleEvent<TestEvent>
    {
        private readonly ILogger _log;
        private readonly ISendCommand _commandSender;

        public TestEventHandler(ILogger log, ISendCommand commandSender)
        {
            _log = log;
            _commandSender = commandSender;
        }

        public async Task Handle(TestEvent evnt, CancellationToken cancellationToken = default)
        {
            _log.Information("Event {eventType} handled", evnt.GetType());
            //await _commandSender.Send(new TestCommand(evnt.Data, causationId: evnt.EventId, evnt.CorrelationId), cancellationToken);
        }
    }
}
