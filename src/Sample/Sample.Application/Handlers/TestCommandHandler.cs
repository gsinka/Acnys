using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Aggregates;
using Acnys.Core.Request.Application;
using Sample.Api.Requests;
using Serilog;

namespace Sample.Application.Handlers
{
    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        private readonly ILogger _log;
        private readonly IPublishEvent _eventPublisher;
        //private readonly IAggregateRepository _aggregateRepository;

        public TestCommandHandler(ILogger log, IPublishEvent eventPublisher/*, IAggregateRepository aggregateRepository*/)
        {
            _log = log;
            _eventPublisher = eventPublisher;
            //_aggregateRepository = aggregateRepository;
        }

        public async Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            //// Get aggregate
            //var aggregate = await _aggregateRepository.GetById<TestAggregateRoot>(Guid.NewGuid());
            //aggregate.TestFunction(command.Data);

            //// Publish uncommitted events
            //var uncommittedEvents = aggregate.GetUncommittedEvents();
            
            await _eventPublisher.Publish(new TestEvent(command.Data, causationId:command.Id, correlationId: command.CorrelationId), cancellationToken);

            _log.Information("Command handled");
        }
    }
}
