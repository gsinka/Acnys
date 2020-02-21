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

        public TestEvent(string data, Guid? eventId = null) : base(eventId)
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

        public TestCommand(string data, Guid? requestId = null) : base(requestId)
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
            var testEvent = new TestEvent(command.Data);

            await _eventPublisher.Publish(
                testEvent,
                new Dictionary<string, object>(arguments.CorrelateTo(command)) 
                    { {"test", "test"}, {"int", 1}}, 
                cancellationToken);
        }
    }
}