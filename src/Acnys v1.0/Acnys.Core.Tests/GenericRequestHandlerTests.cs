using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acnys.AspNet;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure;
using Acnys.Core.Infrastructure.Hosting;
using Acnys.Core.Tests.Helpers;
using Acnys.Core.ValueObjects;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class GenericRequestHandlerTests
    {
        private IHost _host;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly TestCommandHandler _testCommandHandler = new TestCommandHandler();

        public GenericRequestHandlerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            BuildHosts();
        }

        [Fact]
        public async Task Empty_content_should_be_fixed_by_handler()
        {
            var result = await _host.GetTestServer().CreateRequest("api")
                .And(message => message.Content = new StringContent(string.Empty))
                .AddHeader(RequestConstants.DomainType, typeof(TestCommand).AssemblyQualifiedName)
                .PostAsync();

            Assert.True(result.IsSuccessStatusCode);
        }
        
        [Fact]
        public async Task Non_request_fails()
        {
            var result = await _host.GetTestServer().CreateRequest("api")
                .And(message => message.Content = new StringContent(string.Empty))
                .AddHeader(RequestConstants.DomainType, typeof(NonRequest).AssemblyQualifiedName)
                .PostAsync();

            Assert.False(result.IsSuccessStatusCode);
            Assert.True(result.StatusCode == HttpStatusCode.InternalServerError);
        }


        private void BuildHosts()
        {
            _host = new HostBuilder()
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

            _host.Start();

            Log.Logger = new LoggerConfiguration().ConfigureLogForTesting(_testOutputHelper).CreateLogger();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();
        }

        public class NonRequest
        {
        }

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
                Command = command;
                Arguments = arguments;
                return Task.CompletedTask;
            }
        }
    }

}
