using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Helper;
using Acnys.Core.Infrastructure;
using Autofac;
using Autofac.Core;
using Serilog;
using Serilog.Context;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class CommandSenderTests
    {
        public CommandSenderTests(ITestOutputHelper testOutputHelper)
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
        public async Task Loopback_command_sender()
        {
            var localHandler = new LocalCommandHandler();

            var container = BuildContainer(builder =>
                {
                    builder.RegisterInstance(localHandler).AsImplementedInterfaces().SingleInstance();
                    builder.RegisterCommandDispatcher();
                    builder.RegisterLoopbackCommandSender();
                });
            
            var sender = container.Resolve<ISendCommand>();

            var command = new LocalCommand();
            var arguments = new Dictionary<string, object>().UseCorrelationId(Guid.NewGuid());

            await sender.Send(command, arguments);

            Assert.Equal(command, localHandler.Command);
            Assert.Equal(arguments, localHandler.Arguments);
        }

        [Fact]
        public async Task CommandBroker_test()
        {
            var localHandler = new LocalCommandHandler();
            var container = BuildContainer(builder =>
            {
                builder.RegisterInstance(localHandler).AsImplementedInterfaces().SingleInstance();
                builder.RegisterCommandDispatcher();
                builder.RegisterLoopbackCommandSender("loopback");
                builder.RegisterCommandBroker((command, arguments) => command is LocalCommand ? "loopback" : "");
            });

            var sender = container.Resolve<ISendCommand>();
            await sender.Send(new LocalCommand());
            Assert.NotNull(localHandler.Command);
            Assert.Null(localHandler.Arguments);
        }



        private class LocalCommand : Command {
            public LocalCommand(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
            }
        }

        private class LocalCommandHandler : IHandleCommand<LocalCommand>
        {
            public LocalCommand Command;
            public IDictionary<string, object> Arguments;

            public Task Handle(LocalCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                Command = command;
                Arguments = arguments;
                return Task.CompletedTask;
            }
        }
    }
}
