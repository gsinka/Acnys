using System.Collections.Generic;
using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .AddAutofac()
                .AddSerilog((context, config) => config
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{App}] {Message:lj} <-- [{SourceContext}]{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("App", "SMC")
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning))

                .AddHealthChecks((context, builder) => builder.AddCheck("Self", () => HealthCheckResult.Healthy(), new List<string> { "Liveness" }))
                .AddHttpMetrics()

                .AddOpenApiDocumentation(
                    (context, app) => context.Configuration.Bind("Application", app),
                    (context, sso) => context.Configuration.Bind("SingleSignOn", sso))

                .AddRequests()
                .AddRequestValidation()
                .RegisterRequestHandlersFromAssemblyOf<TestEventHandler>()

                .AddRequestSender(request => "http")
                .AddHttpRequestSender(context => "http://localhost:5000/api", "http")

                .AddHttpRequestHandler()

                .AddEventing()
                .RegisterEventHandlersFromAssemblyOf<TestEventHandler>()
            
                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.RegisterType<Setup>().As<IStartable>().SingleInstance();
                    builder.RegisterType<RabbitHostedService>().AsImplementedInterfaces().SingleInstance();
                })

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }


}
