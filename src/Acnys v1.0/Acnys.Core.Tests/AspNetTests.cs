using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Acnys.AspNet;
using Acnys.Core.Abstractions;
using Acnys.Core.Infrastructure.Hosting;
using Acnys.Core.Infrastructure.Serilog;
using Acnys.Core.Services;
using Acnys.Core.Tests.Helpers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace Acnys.Core.Tests
{
    public class AspNetTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private ILifetimeScope _container;


        public AspNetTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Test_host_configuration()
        {
            await BuildHost();

            Assert.IsType<ComputerClock>(_container.Resolve<IClock>());
        }

        private async Task BuildHost()
        {
            var host = new HostBuilder()

                .AddSerilog((context, configuration) => configuration.ConfigureLogForTesting(_testOutputHelper))
                .AddAutofac()
                .AddClock<ComputerClock>()
                .AddHttpRequestHandler()

                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.RegisterType<ComputerClock>().AsImplementedInterfaces();

                })
                .ConfigureServices((context, services) =>
                {

                })
                .ConfigureWebHost(builder =>
                {
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

            await host.RunAsync();

            _container = host.Services.GetAutofacRoot();
        }
    }
}
