using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.AspNet;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Helper;
using Acnys.Core.Infrastructure;
using Acnys.Core.Infrastructure.Hosting;
using Acnys.Core.Tests.Helpers;
using Autofac;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class HttpRequestSenderTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();
        private readonly TestQueryHandler _testQueryHandler = new TestQueryHandler();
        private ISendCommand _commandSender;
        private ISendCommand _badCommandSender;
        private ISendQuery _querySender;
        private ISendQuery _badQuerySender;


        public HttpRequestSenderTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            BuildHosts();
        }

        [Fact]
        public async Task Command_sent_to_http_with_no_exception()
        {
            var command = new TestCommand();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);
            await _commandSender.Send(command, arguments);

            Assert.NotSame(command, _testCommandHandler.Command);
            Assert.Equal(command, _testCommandHandler.Command);
            Assert.Equal(correlationId, _testCommandHandler.Arguments.CorrelationId());
            Assert.Equal(causationId, _testCommandHandler.Arguments.CausationId());
            Assert.Equal(command.RequestId, _testCommandHandler.Arguments.RequestId());
        }
        
        [Fact]
        public async Task Query_sent_to_http_with_no_exception()
        {
            var query = new TestQuery();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);
            await _querySender.Send(query, arguments);

            Assert.NotSame(query, _testQueryHandler.Query);
            Assert.Equal(query, _testQueryHandler.Query);
            Assert.Equal(correlationId, _testQueryHandler.Arguments.CorrelationId());
            Assert.Equal(causationId, _testQueryHandler.Arguments.CausationId());
            Assert.Equal(query.RequestId, _testQueryHandler.Arguments.RequestId());
        }
        
        [Fact]
        public async Task Command_sent_to_http_with_business_exception()
        {
            var command = new TestCommand("business");
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);
            
            var exception = await Assert.ThrowsAsync<BusinessException>(async () => await _commandSender.Send(command, arguments));
            Assert.Equal(100, exception.ErrorCode);
            Assert.Equal("100", exception.Message);
        }

        [Fact]
        public async Task Query_sent_to_http_with_business_exception()
        {
            var query = new TestQuery("business");
            var exception = await Assert.ThrowsAsync<BusinessException>(async () => await _querySender.Send(query));
            Assert.Equal(100, exception.ErrorCode);
            Assert.Equal("100", exception.Message);
        }
        
        [Fact]
        public async Task Command_sent_to_http_with_validation_exception()
        {
            var command = new TestCommand("validation");
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _commandSender.Send(command));
            Assert.Equal("validation failed", exception.Message);
            Assert.NotEmpty(exception.Errors);
        }

        [Fact]
        public async Task Query_sent_to_http_with_validation_exception()
        {
            var query = new TestQuery("validation");
            var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _querySender.Send(query));
            Assert.Equal("validation failed", exception.Message);
            Assert.NotEmpty(exception.Errors);
        }

        [Fact]
        public async Task Command_sent_to_http_with_unknown_exception()
        {
            var command = new TestCommand("invalid");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _commandSender.Send(command));
            Assert.NotEmpty(exception.Message);
            Assert.StartsWith("500", exception.Message);
        }

        [Fact]
        public async Task Query_sent_to_http_with_unknown_exception()
        {
            var query = new TestQuery("invalid");
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _querySender.Send(query));
            Assert.NotEmpty(exception.Message);
            Assert.StartsWith("500", exception.Message);
        }

        [Fact]
        public async Task Command_sent_to_wrong_endpoint_gives_404()
        {
            var command = new TestCommand();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _badCommandSender.Send(command));
            Assert.NotEmpty(exception.Message);
            Assert.StartsWith("404", exception.Message);
        }

        [Fact]
        public async Task Query_sent_to_wrong_endpoint_gives_404()
        {
            var query = new TestQuery();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _badQuerySender.Send(query));
            Assert.NotEmpty(exception.Message);
            Assert.StartsWith("404", exception.Message);
        }

        private void BuildHosts()
        {
            var testHost = new HostBuilder()
                .AddAutofac()
                .UseSerilog((context, configuration) => configuration.ConfigureLogForTesting(_testOutputHelper))
                .AddHttpRequestHandler()
                .ConfigureServices(services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(HttpRequestSenderTests).Assembly).AddControllersAsServices();
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterCommandDispatcher();
                    builder.RegisterQueryDispatcher();
                    builder.RegisterInstance(_testCommandHandler).AsImplementedInterfaces().SingleInstance();
                    builder.RegisterInstance(_testQueryHandler).AsImplementedInterfaces().SingleInstance();
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer();
                    builder.Configure((context, app) =>
                    {
                        app.AddErrorHandling();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHttpRequestHandler("api");
                        });
                    });
                })
                .Build();

            testHost.Start();

            var testServer = testHost.GetTestServer();
            var httpClient = testServer.CreateClient();

            Log.Logger = new LoggerConfiguration().ConfigureLogForTesting(_testOutputHelper).CreateLogger();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            containerBuilder.RegisterHttpCommandSender(testServer.BaseAddress.AbsoluteUri + "api", httpClient);
            containerBuilder.RegisterHttpCommandSender(testServer.BaseAddress.AbsoluteUri + "bad_api", httpClient, "bad_api");
            containerBuilder.RegisterHttpQuerySender(testServer.BaseAddress.AbsoluteUri + "api", httpClient);
            containerBuilder.RegisterHttpQuerySender(testServer.BaseAddress.AbsoluteUri + "bad_api", httpClient, "bad_api");

            var container = containerBuilder.Build();

            _commandSender = container.Resolve<ISendCommand>();
            _querySender = container.Resolve<ISendQuery>();

            _badCommandSender = container.ResolveKeyed<ISendCommand>("bad_api");
            _badQuerySender = container.ResolveKeyed<ISendQuery>("bad_api");
        }

        //private Func<LoggerConfiguration, LoggerConfiguration> ConfigureLog => configuration => configuration
        //    .WriteTo.TestOutput(
        //        _testOutputHelper,
        //        outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{Application}] {Message:lj} [{SourceContext}]{NewLine}{Exception}")
        //    .MinimumLevel.Verbose();

        public class TestCommand : Command
        {
            public string Type { get; }

            public TestCommand(string type = null, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                Type = type;
            }
        }

        public class TestCommandHandler : IHandleCommand<TestCommand>
        {
            public TestCommand Command;
            public IDictionary<string, object> Arguments;

            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                RaiseExpectedError(command.Type);

                Command = command;
                Arguments = arguments;

                return Task.CompletedTask;
            }
        }

        public class TestQuery : Query<string>
        {
            public string Type { get; }

            public TestQuery(string type = null, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                Type = type;
            }
        }

        public class TestQueryHandler : IHandleQuery<TestQuery, string>
        {
            public TestQuery Query;
            public IDictionary<string, object> Arguments;

            public Task<string> Handle(TestQuery query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                RaiseExpectedError(query.Type);

                Query = query;
                Arguments = arguments;

                return Task.FromResult(string.Empty);
            }
        }

        private static void RaiseExpectedError(string type)
        {
            switch (type)
            {
                case null:
                    break;

                case "business":
                    throw new BusinessException(100, "100");

                case "validation":
                    throw new ValidationException("validation failed", new List<ValidationFailure>()
                    {
                        new ValidationFailure("property", "error")
                        {
                            ErrorCode = "code"
                        }
                    });

                default:
                    throw new InvalidOperationException("invalid");
            }
        }
    }
}
