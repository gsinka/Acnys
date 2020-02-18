using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Acnys.Core.Eventing.Abstractions;
using Autofac;
using Microsoft.AspNetCore.Hosting;
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

                .AddRequests()
                .AddRequestValidation()
                .RegisterRequestHandlersFromAssemblyOf<TestEventHandler>()
                
                .AddRequestSender(request => "http")
                .AddHttpRequestSender("http://localhost:5000/api", "http")
                
                .AddHttpRequestHandler()
            
                .AddEventing()
                .RegisterEventHandlersFromAssemblyOf<TestEventHandler>()

                .ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        builder.RegisterType<RabbitHostedService>().AsImplementedInterfaces().SingleInstance();
                    })
                
                
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    
}
