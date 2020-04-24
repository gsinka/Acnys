using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Acnys.AspNet;
using Acnys.Core.Abstractions;
using Acnys.Core.Application.Abstractions;
using Acnys.Core.Infrastructure;
using Acnys.Core.Infrastructure.Hosting;
using Acnys.Core.Infrastructure.Serilog;
using Acnys.Core.Services;
using Acnys.Core.Tests.Helpers;
using Acnys.Core.ValueObjects;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class AspNetTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private ILifetimeScope _container;
        private IHost _host;

        public AspNetTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test_host_configuration()
        {
            await BuildHost(builder =>
            {
                builder.RegisterCommandHandler<TestCommandHandler>();
            });
            
            var result = await _host.GetTestServer().CreateRequest("api")
                .And(message => message.Content = new StringContent(JsonConvert.SerializeObject(new TestCommand())))
                .AddHeader(RequestConstants.DomainType, typeof(TestCommand).AssemblyQualifiedName)
                .PostAsync();

            result.EnsureSuccessStatusCode();
            Assert.IsType<ComputerClock>(_container.Resolve<IClock>());
            
        }

        public class TestCommand : Command {
            public TestCommand() : base(Guid.NewGuid()) { }
        }

        public class TestCommandHandler : IHandleCommand<TestCommand>
        {
            public Task Handle(TestCommand command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private async Task BuildHost(Action<ContainerBuilder> containerBuilder)
        {
            _host = Host.CreateDefaultBuilder()
                .AddAutofac()
                .AddSerilog((context, configuration) => configuration.ConfigureLogForTesting(_testOutputHelper))
                .AddClock<ComputerClock>()
                .AddAspNetControllers()
                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.RegisterType<ComputerClock>().AsImplementedInterfaces();
                    builder.RegisterCommandDispatcher();
                    builder.RegisterQueryDispatcher();
                    containerBuilder(builder);
                })
                .ConfigureServices((context, services) => { services.AddControllers().AddApplicationPart(Assembly.GetEntryAssembly()).AddControllersAsServices(); })
                .ConfigureWebHost(builder =>
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

            await _host.StartAsync();

            _container = _host.Services.GetAutofacRoot(); 
        }
    }
}
