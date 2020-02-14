using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure;
using Acnys.Core.Eventing.Infrastructure.Extensions;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Eventing.Tests
{
    public class EventAwaiterTests
    {
        private readonly IContainer _container;

        public EventAwaiterTests(ITestOutputHelper testOutputHelper)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.RegisterLoopbackEventPublisher();
            builder.RegisterEventDispatcher();
            builder.RegisterEventAwaiterService();

            _container = builder.Build();
        }

        [Fact]
        public async Task Event_awaiter_returns_with_event()
        {
            var svc = _container.Resolve<EventAwaiterService>();
            var testEvent = new TestEvent(correlationId: Guid.NewGuid());
            var awaiter = svc.GetEventAwaiter<TestEvent>((evnt, args) => evnt.CorrelationId == testEvent.CorrelationId, TimeSpan.FromMilliseconds(50), CancellationToken.None);
            await _container.Resolve<IPublishEvent>().Publish(testEvent, new Dictionary<string, object>(), CancellationToken.None);
            Assert.NotNull(await awaiter);
        }

        [Fact]
        public async Task Event_awaiter_returns_no_event_without_publish()
        {
            var svc = _container.Resolve<EventAwaiterService>();
            var testEvent = new TestEvent(correlationId: Guid.NewGuid());
            var awaiter = svc.GetEventAwaiter<TestEvent>((evnt, args) => evnt.CorrelationId == testEvent.CorrelationId, TimeSpan.FromMilliseconds(1), CancellationToken.None);
            await _container.Resolve<IPublishEvent>().Publish(testEvent, new Dictionary<string, object>(), CancellationToken.None);
            Assert.Null(await awaiter);
        }

    }
}
