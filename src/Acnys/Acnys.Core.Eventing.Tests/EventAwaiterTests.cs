using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Extensions;
using Acnys.Core.Eventing.Infrastructure;
using Acnys.Core.Extensions;
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
            var correlationId = Guid.NewGuid();

            var svc = _container.Resolve<EventAwaiterService>();
            var testEvent = new TestEvent();
            var stoppingTokenSource = new CancellationTokenSource(1000);
            var awaiter = svc.GetEventAwaiter<TestEvent>((evnt, args) => correlationId == args.CorrelationId(), stoppingTokenSource.Token);
            await _container.Resolve<IPublishEvent>().Publish(testEvent, new Dictionary<string, object>().UseCorrelationId(correlationId), CancellationToken.None);
            Assert.NotNull(await awaiter);
        }

        [Fact]
        public async Task Event_awaiter_returns_no_event_without_publish()
        {
            var correlationId = Guid.NewGuid();

            var svc = _container.Resolve<EventAwaiterService>();
            var testEvent = new TestEvent();
            var stoppingTokenSource = new CancellationTokenSource(1);
            var awaiter = svc.GetEventAwaiter<TestEvent>((evnt, args) => correlationId == args.CorrelationId(), stoppingTokenSource.Token);
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await awaiter);
            
        }

    }
}
