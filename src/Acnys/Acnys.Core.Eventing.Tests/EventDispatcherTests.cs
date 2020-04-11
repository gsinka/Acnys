using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Extensions;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Eventing.Tests
{
    public class EventDispatcherTests
    {
        private readonly IContainer _container;
        private readonly TestEventHandler _testHandler;

        public EventDispatcherTests(ITestOutputHelper testOutputHelper)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.RegisterLoopbackEventPublisher();
            builder.RegisterEventDispatcher();
            
            _testHandler = new TestEventHandler();
            builder.RegisterInstance(_testHandler).AsImplementedInterfaces();

            _container = builder.Build();
        }

        [Fact]
        public async Task Event_dispatcher_shall_work_without_registered_event_awaiter()
        {
            var testEvent = new TestEvent();
            var dispatcher = _container.Resolve<IDispatchEvent>();
            await dispatcher.Dispatch(testEvent, new Dictionary<string, object>(), CancellationToken.None);
        }

        [Fact]
        public async Task Event_dispatcher_called()
        {
            var testEvent = new TestEvent();
            var args = new Dictionary<string, object>()
            {
                { "test key", "test value" }
            };

            var dispatcher = _container.Resolve<IDispatchEvent>();
            await dispatcher.Dispatch(testEvent, args, CancellationToken.None);

            Assert.NotNull(_testHandler.LastEvent);
            Assert.NotNull(_testHandler.LastArguments);
            Assert.Contains("test key", _testHandler.LastArguments);
            Assert.Equal("test value", _testHandler.LastArguments["test key"]);
        }
        
        [Fact]
        public async Task Dispatch_event_with_no_arguments()
        {
            var testEvent = new TestEvent();
            var dispatcher = _container.Resolve<IDispatchEvent>();
            await dispatcher.Dispatch(testEvent, cancellationToken: CancellationToken.None);

            Assert.NotNull(_testHandler.LastEvent);
            Assert.Null(_testHandler.LastArguments);
        }

        public class TestEventHandler : IHandleEvent<TestEvent>
        {
            public TestEvent LastEvent;
            public IDictionary<string, object> LastArguments;

            public Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                LastEvent = @event;
                LastArguments = arguments;
                return Task.CompletedTask;
            }
        }
    }


}