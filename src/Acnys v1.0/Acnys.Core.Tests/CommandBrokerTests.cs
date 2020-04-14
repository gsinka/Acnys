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
    public class CommandBrokerTests
    {
        public CommandBrokerTests(ITestOutputHelper testOutputHelper)
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
        public async Task Broker_command_to_loopback()
        {
            var testHandler = new TestCommandHandler();

            var container = BuildContainer(builder =>
            {
                builder.RegisterInstance(testHandler).AsImplementedInterfaces().SingleInstance();
                builder.RegisterCommandDispatcher();
                builder.RegisterLoopbackCommandSender("loopback");
                builder.RegisterCommandBroker((command, arguments) => "loopback");
            });

            await container.Resolve<ISendCommand>().Send(new TestCommand());
            Assert.True(testHandler.Called);
        }

        [Fact]
        public async Task Null_sender_key_fails()
        {
            var container = BuildContainer(builder =>
            {
                builder.RegisterCommandBroker((command, arguments) => null);
            });

            await Assert.ThrowsAsync<InvalidOperationException>(async() =>  await container.Resolve<ISendCommand>().Send(new TestCommand()));
        }

        [Fact]
        public async Task Unknown_sender_fails()
        {
            var container = BuildContainer(builder =>
            {
                builder.RegisterCommandBroker((command, arguments) => "loopback");
            });

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await container.Resolve<ISendCommand>().Send(new TestCommand()));
        }

        private class TestCommand : Command
        {
            public TestCommand(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
            }
        }

        private class TestCommandHandler : IHandleCommand<TestCommand>
        {
            public bool Called { get; private set; }

            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                Called = true;
                return Task.CompletedTask;
            }
        }
    }
}