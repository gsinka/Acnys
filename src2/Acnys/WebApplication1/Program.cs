using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Acnys.Core.Eventing;
using Acnys.Core.Eventing.Abstractions;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

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
                .AddSerilog((context, configuration) => configuration
                    .WriteTo.Console()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning))
                .AddRequests()
                .AddRequestValidation()
                
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

    public class TestEvent : Event
    {
        public string Data { get; }

        public TestEvent(string data, Guid? eventId = null, Guid? causationId = null, Guid? correlationId = null) : base(eventId, causationId, correlationId)
        {
            Data = data;
        }
    }

    public class TestEventHandler : IHandleEvent<TestEvent>
    {
        public Task Handle(TestEvent @event, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;

        }
    }
}
