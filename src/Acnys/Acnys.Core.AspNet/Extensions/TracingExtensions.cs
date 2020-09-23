﻿using Acnys.Core.Eventing.Infrastructure.Extensions;
using Acnys.Core.Request.Infrastructure.Extensions;
using Autofac;
using Jaeger;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
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

        public static IHostBuilder AddTracing(this IHostBuilder hostBuilder, Configuration.SenderConfiguration config=null , bool addOpenTracing = false)
        {

            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
            {
                containerBuilder.AddEventTracingBehaviour();
                containerBuilder.AddCommandTracingBehaviour();
            }).
            ConfigureServices((context, services) =>
            {
                services.AddSingleton<ITracer>(serviceProvider =>
                {
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                    Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory)
                        .RegisterSenderFactory<ThriftSenderFactory>();
                    if(config == null)
                    {
                        config = new Configuration.SenderConfiguration(loggerFactory)
                        .WithAgentHost("localhost")
                        .WithAgentPort(6831);
                    }
                    var udpSender = config.GetSender();

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

            });
        }
    }
}
