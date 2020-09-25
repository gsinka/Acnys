using Acnys.Core.Eventing.Infrastructure.Extensions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using System;
using System.Reflection;

namespace Acnys.Core.AspNet.Extensions
{
    public static class TracingExtensions
    {

        public static IHostBuilder AddTracing(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection> configuration = null, bool addOpenTracing = false)
        {

            if (configuration == null) configuration = (context, services) =>
             {
                 services.AddSingleton<ITracer>(serviceProvider =>
                 {
                     var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                     var log = serviceProvider.GetRequiredService<Serilog.ILogger>();

                     Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                         .RegisterSenderFactory<ThriftSenderFactory>();

                     var endpoint = context.Configuration.GetValue<string>("Tracing:Host") ?? "localhost";
                     var port = context.Configuration.GetValue<int?>("Tracing:Port") ?? 6831;
                     log.Debug($"Tracing added with endpoint: {endpoint}");

                     var reporter = new RemoteReporter.Builder()
                      .WithLoggerFactory(loggerFactory)
                      .WithSender(new UdpSender(endpoint, port, 0))
                      .Build();

                     var tracer = new Tracer.Builder(Assembly.GetEntryAssembly().GetName().Name)
                      .WithLoggerFactory(loggerFactory)
                      .WithSampler(new ConstSampler(true))
                      .WithReporter(reporter)
                      .Build();


                     if (!GlobalTracer.IsRegistered())
                     {
                         GlobalTracer.Register(tracer);
                     }

                     return tracer;
                 });

                 if (addOpenTracing)
                 {
                     // Useless default http tracing
                     services.AddOpenTracing();
                 }

             };

            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.AddEventTracingBehaviour();
                containerBuilder.AddCommandTracingBehaviour();
                containerBuilder.AddRequestTracingBehaviour();
            }).
            ConfigureServices(configuration);
        }
    }
}



/*
 
(context, services) =>
            {
                services.AddSingleton<ITracer>(serviceProvider =>
                {
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                    Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                        .RegisterSenderFactory<ThriftSenderFactory>();

                    var udpSender = new Configuration.SenderConfiguration(loggerFactory)
                        .WithAgentHost(context.Configuration.GetValue<string>("Tracing:Host") ?? "localhost")
                        .WithAgentPort(context.Configuration.GetValue<int?>("Tracing:Port") ?? 6831)
                        .GetSender();

                    //var config = Configuration.FromEnv(loggerFactory);

                    var tracer = new Tracer.Builder(Assembly.GetEntryAssembly().GetName().Name)
                        .WithLoggerFactory(loggerFactory)
                        .WithSampler(new ConstSampler(true))
                        .Build();

                    if (!GlobalTracer.IsRegistered())
                    {
                        GlobalTracer.Register(tracer);
                    }

                    return tracer;
                });

                if (addOpenTracing)
                {
                    // Useless default http tracing
                    services.AddOpenTracing();
                }

            }
 */