using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Extensions;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Serilog;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace Acnys.Core.Request.Tests.UnitTests
{
    public class Command_Tests
    {
        private readonly IContainer _container;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();

        public Command_Tests(ITestOutputHelper testOutputHelper)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            
            builder.RegisterLoopbackRequestSender();
            builder.RegisterCommandDispatcher();
            builder.RegisterQueryDispatcher();
            builder.RegisterInstance(_testCommandHandler).AsImplementedInterfaces().SingleInstance();

            _container = builder.Build();
        }

        [Fact]
        public async Task Test1()
        {
            var sender = _container.Resolve<ISendRequest>();

            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();

            var args = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);

            await sender.Send(new TestCommand(), args, CancellationToken.None);

            Assert.NotNull(_testCommandHandler.Command);
            Assert.NotNull(_testCommandHandler.Arguments);

            Assert.Equal(correlationId, args.CorrelationId());
            Assert.Equal(causationId, args.CausationId());
        }
    }
}
