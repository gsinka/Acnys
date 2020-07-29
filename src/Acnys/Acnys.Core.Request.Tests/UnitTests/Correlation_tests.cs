using System;
using System.Collections.Generic;
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
    public class Correlation_tests
    {
        private readonly IContainer _container;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();

        public Correlation_tests(ITestOutputHelper testOutputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(testOutputHelper).MinimumLevel.Verbose().CreateLogger();
            
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            builder.RegisterCommandDispatcher();
            builder.RegisterQueryDispatcher();
            builder.RegisterRequestSender((request, args) => "loopback");
            builder.RegisterLoopbackRequestSender("loopback");
            builder.RegisterInstance(_testCommandHandler).AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new TestQueryHandler()).AsImplementedInterfaces().SingleInstance();

            _container = builder.Build();
        }

        [Fact]
        public async Task Correlation_id_passed_in_loopback_command_dispatch()
        {
            var requestSender = _container.Resolve<ISendRequest>();

            var command = new TestCommand();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);

            await requestSender.Send(command, arguments);

            Assert.Equal(correlationId, _testCommandHandler.Arguments.CorrelationId());
            Assert.Equal(causationId, _testCommandHandler.Arguments.CausationId());
        }

        [Fact]
        public async Task Correlation_id_passed_in_loopback_query_dispatch()
        {
            var requestSender = _container.Resolve<ISendRequest>();

            var query = new TestQuery();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);

            var returnedArguments = await requestSender.Send(query, arguments);

            Assert.Equal(correlationId, returnedArguments.CorrelationId());
            Assert.Equal(causationId, returnedArguments.CausationId());
        }
    }
}