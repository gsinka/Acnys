using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class CommandDispatcherTests
    {
        private readonly IContainer _container;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();

        public CommandDispatcherTests(ITestOutputHelper outputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(outputHelper).MinimumLevel.Verbose().CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            builder.RegisterCommandDispatcher();
            builder.RegisterInstance(_testCommandHandler).AsImplementedInterfaces().SingleInstance();

            _container = builder.Build();
        }

        [Fact]
        public async Task Test_command_dispatcher_with_valid_command()
        {
            var dispatcher = _container.Resolve<IDispatchCommand>();
            
            var command = new TestCommand();
            var correlationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId);

            await dispatcher.Dispatch(command, arguments);

            Assert.Equal(command, _testCommandHandler.Command);
            Assert.Equal(arguments, _testCommandHandler.Arguments);
        }
        
        [Fact]
        public async Task Test_command_dispatcher_with_invalid_command()
        {
            var dispatcher = _container.Resolve<IDispatchCommand>();
            var command = new TestCommand(true);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await dispatcher.Dispatch(command));
        }

        [Fact]
        public async Task Multiple_handlers_registered_shall_raise_an_error()
        {
            await using var scope = _container.BeginLifetimeScope(builder => builder.RegisterType<DuplicatedTestCommandHandler>().AsImplementedInterfaces());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await scope.Resolve<IDispatchCommand>().Dispatch(new TestCommand()));
        }


        [Fact]
        public async Task No_handler_registered_shall_raise_an_error()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _container.Resolve<IDispatchCommand>().Dispatch(new TestCommandForNoHandler()));
        }

        private class TestCommand : Command
        {
            public bool ThrowException { get; }

            public TestCommand(bool throwException = false)
            {
                ThrowException = throwException;
            }
        }

        private class TestCommandForNoHandler : Command { }

        private class TestCommandHandler : IHandleCommand<TestCommand>
        {
            public TestCommand Command;
            public IDictionary<string, object> Arguments;

            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                if (command.ThrowException) throw new InvalidOperationException();

                Command = command;
                Arguments = arguments;

                return Task.CompletedTask;
            }
        }

        private class DuplicatedTestCommandHandler : IHandleCommand<TestCommand>
        {
            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
