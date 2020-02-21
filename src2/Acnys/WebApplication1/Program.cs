using System;
using Acnys.Core.AspNet;
using Acnys.Core.AspNet.Eventing;
using Acnys.Core.AspNet.RabbitMQ;
using Acnys.Core.AspNet.Request;
using Autofac;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WebApplication1
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var app = AppBuilder.Build(args, hostBuilder =>
                {
                    hostBuilder

                        .PrebuildDefaultApp(AppBuilderExtensions.DefaultLogger("Test application", LogEventLevel.Verbose))

                        .RegisterRequestHandlersFromAssemblyOf<TestEventHandler>()
                        .RegisterEventHandlersFromAssemblyOf<TestEventHandler>()

                        .AddRequestSender(request => "http")
                        .AddHttpRequestSender(context => "http://localhost:5000/api", "http")

                        .AddRabbit((context, factory) =>
                        {
                            factory.Uri = new Uri(context.Configuration["Rabbit:Uri"]);
                            factory.AutomaticRecoveryEnabled = true;

                        }, "test", "test")

                        .ConfigureContainer<ContainerBuilder>((context, builder) =>
                        {
                            builder.RegisterType<Setup>().As<IStartable>().SingleInstance();
                        })

                        ;


                });

                Log.ForContext<Program>().Information("Running application");

                app.Run();

                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application failed");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
