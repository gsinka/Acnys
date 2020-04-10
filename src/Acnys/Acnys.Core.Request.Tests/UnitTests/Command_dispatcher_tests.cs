
// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Dispatchers;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Request.Tests.UnitTests
{
    public class Command_dispatcher_tests
    {
        private IContainer _container;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();

        public Command_dispatcher_tests(ITestOutputHelper testOutputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.RegisterCommandDispatcher();
            builder.RegisterInstance(_testCommandHandler).AsImplementedInterfaces().SingleInstance();

            _container = builder.Build();
        }

        [Fact]
        public async Task Command_dispatch_works()
        {
            var dispatcher = _container.Resolve<IDispatchCommand>();
            
            var command = new TestCommand();
            var arguments = new Dictionary<string, object>()
            {
                {"key_1", "value_1"},
                {"key_2", "value_2"},
            };

            await dispatcher.Dispatch(command, arguments);
            Assert.Equal(command, _testCommandHandler.Command);
            Assert.Equal(arguments, _testCommandHandler.Arguments);
        }
    }
}