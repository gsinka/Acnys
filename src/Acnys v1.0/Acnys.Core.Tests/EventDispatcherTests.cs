using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Helper;
using Acnys.Core.Infrastructure;
using Acnys.Core.Infrastructure.Dispatcher;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class EventDispatcherTests
    {
        private readonly IContainer _container;
        private readonly TestEventHandler _testEventHandler = new TestEventHandler();

        public EventDispatcherTests(ITestOutputHelper outputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(outputHelper).MinimumLevel.Verbose().CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.RegisterEventDispatcher();
            builder.RegisterInstance(_testEventHandler).AsImplementedInterfaces().SingleInstance();
            _container = builder.Build();
        }

        [Fact]
        public async Task Test_event_dispatcher_with_valid_event()
        {
            var dispatcher = _container.Resolve<IDispatchEvent>();
            
            var evnt = new TestEvent();
            var correlationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId);

            await dispatcher.Dispatch(evnt, arguments);

            Assert.Equal(evnt, _testEventHandler.Event);
            Assert.Equal(arguments, _testEventHandler.Arguments);
        }
        
        [Fact]
        public async Task Test_event_dispatcher_with_invalid_event()
        {
            var dispatcher = _container.Resolve<IDispatchEvent>();
            var evnt = new TestEvent(true);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await dispatcher.Dispatch(evnt));
        }

        [Fact]
        public async Task Test_event_dispatcher_with_no_handler()
        {
            var dispatcher = _container.Resolve<IDispatchEvent>();
            var evnt = new TestEventForNoHandler();
            await dispatcher.Dispatch(evnt);
        }

        public class TestEvent : Event
        {
            public bool ThrowException { get; }

            public TestEvent(bool throwException = false, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                ThrowException = throwException;
            }
        }

        public class TestEventForNoHandler : Event
        {
            public TestEventForNoHandler(Guid? eventId = null) : base(eventId ?? Guid.NewGuid()) { }
        }

        public class TestEventHandler : IHandleEvent<TestEvent>
        {
            public TestEvent Event;
            public IDictionary<string, object> Arguments;

            public Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                if (@event.ThrowException) throw new InvalidOperationException();

                Event = @event;
                Arguments = arguments;

                return Task.CompletedTask;
            }
        }
    }
}