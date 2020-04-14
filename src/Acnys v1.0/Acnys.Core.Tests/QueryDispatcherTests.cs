using System;
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
    public class QueryDispatcherTests
    {
        private readonly IContainer _container;
        private readonly TestQueryHandler _testQueryHandler = new TestQueryHandler();

        public QueryDispatcherTests(ITestOutputHelper outputHelper)
        {
            var builder = new ContainerBuilder();

            Log.Logger = new LoggerConfiguration().WriteTo.TestOutput(outputHelper).MinimumLevel.Verbose().CreateLogger();
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            builder.RegisterQueryDispatcher();
            builder.RegisterInstance(_testQueryHandler).AsImplementedInterfaces().SingleInstance();

            _container = builder.Build();
        }

        [Fact]
        public async Task Test_query_dispatcher_with_valid_command()
        {
            var dispatcher = _container.Resolve<IDispatchQuery>();
            
            var query = new TestQuery();
            var correlationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId);

            var result = await dispatcher.Dispatch(query, arguments);

            Assert.Equal(arguments, result);
        }
        
        [Fact]
        public async Task Test_query_dispatcher_with_invalid_command()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _container.Resolve<IDispatchQuery>().Dispatch(new TestQuery(true)));
        }

        [Fact]
        public async Task Multiple_handlers_registered_shall_raise_an_error()
        {
            await using var scope = _container.BeginLifetimeScope(builder => builder.RegisterType<DuplicatedTestQueryHandler>().AsImplementedInterfaces());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await scope.Resolve<IDispatchQuery>().Dispatch(new TestQuery()));
        }

        [Fact]
        public async Task No_handler_registered_shall_raise_an_error()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _container.Resolve<IDispatchQuery>().Dispatch(new TestQueryForNoHandler()));
        }

        public class TestQuery : Query<IDictionary<string, object>>
        {
            public bool ThrowException { get; }

            public TestQuery(bool throwException = false, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                ThrowException = throwException;
            }
        }

        public class TestQueryForNoHandler : Query<object> {

            public TestQueryForNoHandler(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
            }
        }

        public class TestQueryHandler : IHandleQuery<TestQuery, IDictionary<string, object>>
        {
            public Task<IDictionary<string, object>> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                if (query.ThrowException) throw new InvalidOperationException();
                return Task.FromResult(arguments);
            }
        }

        public class DuplicatedTestQueryHandler : IHandleQuery<TestQuery, IDictionary<string, object>>
        {
            public Task<IDictionary<string, object>> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                throw new InvalidOperationException();
            }
        }
    }
}