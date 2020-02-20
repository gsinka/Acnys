using System;
using System.Collections.Generic;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.RabbitMQ;
using Acnys.Core.RabbitMQ.Extensions;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Serilog;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public static class RabbitExtensions
    {
        public static IHostBuilder AddRabbit(this IHostBuilder hostBuilder, Action<HostBuilderContext, ConnectionFactory> connectionBuilder, string exchange, string queue, bool autoStartListener = true)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.AddRabbitConnection(factory => connectionBuilder(context, factory));
                    builder.AddRabbitEventPublisher(exchange);
                    builder.AddRabbitEventListener(queue);
                    builder.RegisterType<RabbitService>().AsImplementedInterfaces().SingleInstance();

                    if (autoStartListener) builder.AutoStartRabbitEventListeners();
                })

                    .ConfigureServices((context, services) => services
                        .AddHealthChecks()
                            .AddCheck<RabbitEventListenerHealthCheck>("RabbitMQ event listener", tags: new [] { "Readiness" }))
                
                ;
        }
    }
}
