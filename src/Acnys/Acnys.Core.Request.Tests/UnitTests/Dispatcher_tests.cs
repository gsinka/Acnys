using System.Collections.Generic;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Acnys.Core.Request.Tests.UnitTests
{
    public class Dispatcher_tests
    {
        private readonly IContainer _container;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();
        private readonly TestQueryHandler _testQueryHandler = new TestQueryHandler();

        public Dispatcher_tests(ITestOutputHelper testOutputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.RegisterCommandDispatcher();
            builder.RegisterQueryDispatcher();
            builder.RegisterInstance(_testCommandHandler).AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(_testQueryHandler).AsImplementedInterfaces().SingleInstance();

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

        [Fact]
        public async Task Query_dispatch_works()
        {
            var dispatcher = _container.Resolve<IDispatchQuery>();
            
            var query = new TestQuery();
            var arguments = new Dictionary<string, object>()
            {
                {"key_1", "value_1"},
                {"key_2", "value_2"},
            };

            var result = await dispatcher.Dispatch(query, arguments);

            Assert.Equal(result, arguments);
        }
    }
}