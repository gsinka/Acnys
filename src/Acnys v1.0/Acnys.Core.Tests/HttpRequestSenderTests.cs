using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.AspNet;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Helper;
using Acnys.Core.Infrastructure;
using Acnys.Core.Infrastructure.Hosting;
using Acnys.Core.Infrastructure.Sender;
using Autofac;
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

        public HttpRequestSenderTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(_testOutputHelper, outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{Application}] {Message:lj} [{SourceContext}]{NewLine}{Exception}")
                .MinimumLevel.Verbose()
                .CreateLogger();
        }

        private IHostBuilder BuildTestHost(Action<ContainerBuilder> containerBuilder)
        {
            return new HostBuilder()
                .AddAutofac()
                .UseSerilog((context, configuration) => configuration
                    .WriteTo.TestOutput(_testOutputHelper, outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{Application}] {Message:lj} [{SourceContext}]{NewLine}{Exception}")
                    .MinimumLevel.Verbose())
                .AddHttpRequestHandler()
                .ConfigureServices(services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(HttpRequestSenderTests).Assembly).AddControllersAsServices();
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterCommandDispatcher();
                    builder.RegisterQueryDispatcher();
                    containerBuilder(builder);
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer();
                    builder.Configure((context, app) =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapHttpRequestHandler("api");
                        });
                    });
                });
        }

        [Fact]
        public async Task Command_sent_to_http()
        {
            var testHandler = new TestCommandHandler();

            var testHost = await BuildTestHost(builder =>
            {
                builder.RegisterInstance(testHandler).AsImplementedInterfaces().SingleInstance();
            }).StartAsync();
            
            var testServer = testHost.GetTestServer();
            var httpClient = testServer.CreateClient();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            containerBuilder.RegisterHttpCommandSender(testServer.BaseAddress.AbsoluteUri + "api", httpClient);

            var container = containerBuilder.Build();

            var sender = container.Resolve<ISendCommand>();
            var command = new TestCommand();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);
            await sender.Send(command, arguments);

            Assert.NotSame(command, testHandler.Command);
            Assert.Equal(command, testHandler.Command);
            Assert.Equal(correlationId, testHandler.Arguments.CorrelationId());
            Assert.Equal(causationId, testHandler.Arguments.CausationId());
            Assert.Equal(command.RequestId, testHandler.Arguments.RequestId());
        }
        
        [Fact]
        public async Task Command_sent_to_http_with_business_exception()
        {
            var testHandler = new TestCommandHandler();

            var testHost = await BuildTestHost(builder =>
            {
                builder.RegisterInstance(testHandler).AsImplementedInterfaces().SingleInstance();
            }).StartAsync();
            
            var testServer = testHost.GetTestServer();
            var httpClient = testServer.CreateClient();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            containerBuilder.RegisterHttpCommandSender(testServer.BaseAddress.AbsoluteUri + "api", httpClient);

            var container = containerBuilder.Build();

            var sender = container.Resolve<ISendCommand>();
            var command = new TestCommand(HttpStatusCode.BadRequest, "100");
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);
            
            var exception = await Assert.ThrowsAsync<HttpRequestSenderException>(async () => await sender.Send(command, arguments));
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal("100", exception.Message);
        }
        
        [Fact]
        public async Task Command_sent_to_wrong_endpoint_gives_404()
        {
            var testHost = await new HostBuilder()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer();
                    builder.Configure((context, app) => { });
                }).StartAsync();

            var testServer = testHost.GetTestServer();
            var httpClient = testServer.CreateClient();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
            containerBuilder.RegisterHttpCommandSender(testServer.BaseAddress.AbsoluteUri + "missing_api", httpClient);

            var container = containerBuilder.Build();

            var sender = container.Resolve<ISendCommand>();
            var command = new TestCommand();
            var correlationId = Guid.NewGuid();
            var causationId = Guid.NewGuid();
            var arguments = new Dictionary<string, object>().UseCorrelationId(correlationId).UseCausationId(causationId);
            
            var exception = await Assert.ThrowsAsync<HttpRequestSenderException>(async () => await sender.Send(command, arguments));
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        public class TestCommand : Command
        {
            public HttpStatusCode StatusCode { get; }
            public string Message { get; }

            public TestCommand(HttpStatusCode statusCode = HttpStatusCode.OK, string message = null, Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
                StatusCode = statusCode;
                Message = message;
            }
        }

        public class TestCommandHandler : IHandleCommand<TestCommand>
        {
            public TestCommand Command;
            public IDictionary<string, object> Arguments;

            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                Command = command;
                Arguments = arguments;

                switch (command.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new BusinessException(400, command.Message);

                    default:
                        return Task.CompletedTask;
                }
            }
        }
    }
}
