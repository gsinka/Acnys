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
using Acnys.Core.Tracing;

namespace Acnys.Core.AspNet.Tracing
{
    public static class TracingExtensions
    {

        public static IHostBuilder AddTracing(this IHostBuilder hostBuilder, Func<IConfiguration, string> hostConfiguration, Func<IConfiguration, int> portConfiguration)
        {
            return hostBuilder.AddTracing((options, services) =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                var host = hostConfiguration(configuration);
                var port = portConfiguration(configuration);

                options.SenderFactory = new ThriftSenderFactory();
                options.Sender = string.IsNullOrWhiteSpace(host) ? (ISender)new NoopSender() : new UdpSender(host, port, 0);
            });
        }


        public static IHostBuilder AddTracing(this IHostBuilder hostBuilder, Action<TracingOptions, IServiceProvider> configuration)
        {
            hostBuilder.ConfigureServices((services) => 
            {
                services.AddSingleton<ITracer>(services =>
                {
                    var options = new TracingOptions();
                    configuration(options, services);

                    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(typeof(TracingExtensions));

                    Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                        .RegisterSenderFactory(options.SenderFactory);

                    var reporter = new RemoteReporter.Builder()
                        .WithLoggerFactory(loggerFactory)
                        .WithSender(options.Sender)
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

                    logger.LogDebug("Tracing added");

                     return tracer;
                });
            });

            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.AddTracing();
            });
        }
    }
}

