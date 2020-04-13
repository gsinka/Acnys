using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class QueryBrokerTests
    {
        public QueryBrokerTests(ITestOutputHelper testOutputHelper)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();
        }

        private static IContainer BuildContainer(Action<ContainerBuilder> builder)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder(containerBuilder);
            return containerBuilder.Build();
        }

        [Fact]
        public async Task Broker_query_to_loopback()
        {
            var testHandler = new TestQueryHandler();

            var container = BuildContainer(builder =>
            {
                builder.RegisterInstance(testHandler).AsImplementedInterfaces().SingleInstance();
                builder.RegisterQueryDispatcher();
                builder.RegisterLoopbackQuerySender("loopback");
                builder.RegisterQueryBroker((command, arguments) => "loopback");
            });

            Assert.True(await container.Resolve<ISendQuery>().Send(new TestQuery()));
        }

        [Fact]
        public async Task Null_sender_key_fails()
        {
            var container = BuildContainer(builder =>
            {
                builder.RegisterQueryBroker((query, arguments) => null);
            });

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await container.Resolve<ISendQuery>().Send(new TestQuery()));
        }

        [Fact]
        public async Task Unknown_sender_fails()
        {
            var container = BuildContainer(builder =>
            {
                builder.RegisterQueryBroker((query, arguments) => "loopback");
            });

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await container.Resolve<ISendQuery>().Send(new TestQuery()));
        }

        public class TestQuery : Query<bool>{ }

        public class TestQueryHandler : IHandleQuery<TestQuery, bool>
        {
            public Task<bool> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(true);
            }
        }
    }
}