using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request;
using Acnys.Core.Request.Abstractions;

namespace WebApplication1
{
    public class TestEvent : Event
    {
        public string Data { get; }

        public TestEvent(string data, Guid? eventId = null, Guid? causationId = null, Guid? correlationId = null) : base(eventId, causationId, correlationId)
        {
            Data = data;
        }
    }

    public class TestEventHandler : IHandleEvent<TestEvent>
    {
        public Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;

        }
    }

    public class TestCommand : Command
    {
        public string Data { get; }

        public TestCommand(string data, Guid? requestId = null, Guid? causationId = null, Guid? correlationId = null) : base(requestId, causationId, correlationId)
        {
            Data = data;
        }
    }

    public class TestCommandHandler : IHandleCommand<TestCommand>
    {
        private readonly IPublishEvent _eventPublisher;

        public TestCommandHandler(IPublishEvent eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            await _eventPublisher.Publish(
                new TestEvent(command.Data, command.RequestId, command.CorrelationId), 
                new Dictionary<string, object> { {"test", "test"}, {"int", 1}}, 
                cancellationToken);

        }
    }
}