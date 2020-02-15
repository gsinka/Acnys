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
        }

        private readonly Action<ConnectionFactory> CloudConnectionFactory = (factory => {
            factory.Uri = new Uri("amqp://nshjcrft:Wt982f1uasRnmN80__KBvKnVJPsvBJIP@hawk.rmq.cloudamqp.com/nshjcrft");
            factory.AutomaticRecoveryEnabled = true;
        });

        [Fact]
        public void Test_connection_create()
        {
            var builder = new ContainerBuilder();

            builder.AddRabbitConnection(CloudConnectionFactory);
            builder.AddRabbitConnection(CloudConnectionFactory, "key");

            var container = builder.Build();

            var connection = container.Resolve<IConnection>();
            Assert.True(connection.IsOpen);

            var keyedConnection = container.ResolveKeyed<IConnection>("key");
            Assert.True(keyedConnection.IsOpen);

            var eventPublisher = new EventPublisher(connection, "test", EventPublisher.Default);
            
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
            var eventPublisher = new EventPublisher(connection, "test", EventPublisher.Default);

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
            builder.RegisterEventHandlersFromAssemblyOf<TestEventHandler>();
            builder.AddRabbitConnection(CloudConnectionFactory);

            var container = builder.Build();

            var connection = container.Resolve<IConnection>();

            var eventPublisher = new EventPublisher(connection, "test", EventPublisher.Default);

            eventPublisher.Publish(
                new TestEvent("test data", correlationId: Guid.NewGuid(), causationId: Guid.NewGuid()),
                new Dictionary<string, object>()
                {
                    { "key1", "value1" },
                    { "key2", "value2" },
                });

            var listener = new EventListener(
                container.Resolve<ILogger>(),
                connection,
                container.Resolve<IDispatchEvent>(),
                "test",
                string.Empty,
                new Dictionary<string, object>(),
                EventListener.Default);

            Thread.Sleep(1000);
            Assert.True(true);
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
        public Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
