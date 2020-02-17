using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Eventing;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Eventing.Infrastructure.Extensions;
using Acnys.Core.RabbitMQ.Extensions;
using Autofac;
using RabbitMQ.Client;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.RabbitMQ.Tests
{
    public class ConnectionTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ConnectionTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();
        }

        private readonly Action<ConnectionFactory> CloudConnectionFactory = (factory => {
            factory.Uri = new Uri("amqp://nshjcrft:Wt982f1uasRnmN80__KBvKnVJPsvBJIP@hawk.rmq.cloudamqp.com/nshjcrft");
            factory.AutomaticRecoveryEnabled = true;
        });

        [Fact]
        public void Test_connection_create()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.AddRabbitConnection(CloudConnectionFactory);
            builder.AddRabbitConnection(CloudConnectionFactory, "key");

            var container = builder.Build();

            var connection = container.Resolve<IConnection>();
            Assert.True(connection.IsOpen);

            var keyedConnection = container.ResolveKeyed<IConnection>("key");
            Assert.True(keyedConnection.IsOpen);

            var eventPublisher = new EventPublisher(container.Resolve<ILogger>(), connection, "test", EventPublisher.DefaultContextBuilder);
            
            eventPublisher.Publish(
                new TestEvent("test data", correlationId: Guid.NewGuid(), causationId: Guid.NewGuid()), 
                new Dictionary<string, object>()
                {
                    { "key1", "value1" },
                    { "key2", "value2" },
                });
        }

        [Fact]
        public void Test_event_publisher()
        {
            var builder = new ContainerBuilder();
            builder.AddRabbitConnection(CloudConnectionFactory);
            var container = builder.Build();

            var connection = container.Resolve<IConnection>();
            var eventPublisher = new EventPublisher(container.Resolve<ILogger>(), connection, "test", EventPublisher.DefaultContextBuilder);

            eventPublisher.Publish(
                new TestEvent("test data", correlationId: Guid.NewGuid(), causationId: Guid.NewGuid()),
                new Dictionary<string, object>()
                {
                    { "key1", "value1" },
                    { "key2", "value2" },
                });
        }

        [Fact]
        public void Test_event_listener()
        {
            var builder = new ContainerBuilder();
            
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(_testOutputHelper).MinimumLevel.Verbose().CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            builder.RegisterEventDispatcher();

            builder.RegisterType<RabbitService>().SingleInstance().AsSelf();
            builder.AddRabbitConnection(CloudConnectionFactory);

            var handler1 = new TestEventHandler(new ManualResetEvent(false));
            builder.RegisterInstance(handler1).SingleInstance().AsImplementedInterfaces();
            var handler2 = new TestEventHandler(new ManualResetEvent(false));
            builder.RegisterInstance(handler2).SingleInstance().AsImplementedInterfaces();

            var container = builder.Build();

            var rabbit = container.Resolve<RabbitService>();

            rabbit.CreateExchange("test", autoDelete:true);
            rabbit.CreateQueue("test", autoDelete: true);
            rabbit.CreateQueue("test2", autoDelete: true);
            rabbit.Bind("test", "test");
            rabbit.Bind("test2", "test");
            rabbit.AddEventListener("test");
            rabbit.AddEventListener("test2");
            
            var eventPublisher = new EventPublisher(container.Resolve<ILogger>(), rabbit.Connection, "test", EventPublisher.DefaultContextBuilder);

            var testEvent = new TestEvent("test data", correlationId: Guid.NewGuid(), causationId: Guid.NewGuid());

            eventPublisher.Publish(
                testEvent,
                new Dictionary<string, object>()
                {
                    { "key1", "value1" },
                    { "key2", "value2" },
                });

            Assert.True(handler1.ManualResetEvent.WaitOne(1000) && handler2.ManualResetEvent.WaitOne(1000));

            Assert.NotNull(handler1.Event);
            Assert.NotNull(handler1.Args);

            Assert.Equal(testEvent.EventId, handler1.Event.EventId);
            Assert.Equal(testEvent.CorrelationId, handler1.Event.CorrelationId);
            Assert.Equal(testEvent.CausationId, handler1.Event.CausationId);
        }
    }

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
        public readonly ManualResetEvent ManualResetEvent;
        public TestEvent Event { get; private set; } = null;
        public IDictionary<string, object> Args { get; private set; } = null;

        public TestEventHandler(ManualResetEvent manualResetEvent)
        {
            ManualResetEvent = manualResetEvent;
        }

        public Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            Event = @event;
            Args = arguments;
            ManualResetEvent.Set();
            return Task.CompletedTask;
        }
    }
}
