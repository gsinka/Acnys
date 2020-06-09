using System;
using Acnys.Core.RabbitMQ;
using Acnys.Core.RabbitMQ.Extensions;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Acnys.Core.AspNet.RabbitMQ
{
    public static class RabbitExtensions
    {
        public static IHostBuilder AddRabbit(this IHostBuilder hostBuilder, Action<HostBuilderContext, ConnectionFactory> connectionBuilder, string exchange, string queue, bool requeueOnNack = false, bool autoStartListener = true, string consumerTag = "", int consumerCount = 1)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
                {
                    builder.AddRabbitConnection(factory => connectionBuilder(context, factory));
                    builder.AddRabbitEventPublisher(exchange);
                    builder.AddRabbitEventListener(queue, requeueOnNack, consumerCount, consumerTag);
                    builder.RegisterType<RabbitService>().AsImplementedInterfaces().SingleInstance();

                    if (autoStartListener) builder.AutoStartRabbitEventListeners();
                })

                    .ConfigureServices((context, services) => services
                        .AddHealthChecks()
                            .AddCheck<RabbitEventListenerHealthCheck>("RabbitMQ event listener", tags: new [] { "Readiness" }))
                ;
        }
        
        public static IHostBuilder AddRabbit(this IHostBuilder hostBuilder, Action<HostBuilderContext, ConnectionFactory> connectionBuilder, Func<HostBuilderContext, (string exchange, string queue, bool autoStartListener, string consumerTag, int consumerCount)> properties)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        var (exchange, queue, autoStartListener, consumerTag, consumerCount) = properties(context);
                        builder.AddRabbitConnection(factory => connectionBuilder(context, factory));
                        builder.AddRabbitEventPublisher(exchange);
                        builder.AddRabbitEventListener(queue, consumerCount: consumerCount, consumerTag: consumerTag);
                        builder.RegisterType<RabbitService>().AsImplementedInterfaces().SingleInstance();

                        if (autoStartListener) builder.AutoStartRabbitEventListeners();
                    })

                    .ConfigureServices((context, services) => services
                        .AddHealthChecks()
                        .AddCheck<RabbitEventListenerHealthCheck>("RabbitMQ event listener", tags: new[] { "Readiness" }))
                ;
        }

        public static IHostBuilder AddRabbit(this IHostBuilder hostBuilder, Action<HostBuilderContext, ConnectionFactory> connectionBuilder, Func<HostBuilderContext, (string exchange, string queue, bool requeueOnNack, bool autoStartListener, string consumerTag, int consumerCount)> properties)
        {
            return hostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        var (exchange, queue, requeueOnNack, autoStartListener, consumerTag, consumerCount) = properties(context);
                        builder.AddRabbitConnection(factory => connectionBuilder(context, factory));
                        builder.AddRabbitEventPublisher(exchange);
                        builder.AddRabbitEventListener(queue, requeueOnNack, consumerCount, consumerTag);
                        builder.RegisterType<RabbitService>().AsImplementedInterfaces().SingleInstance();

                        if (autoStartListener) builder.AutoStartRabbitEventListeners();
                    })

                    .ConfigureServices((context, services) => services
                        .AddHealthChecks()
                        .AddCheck<RabbitEventListenerHealthCheck>("RabbitMQ event listener", tags: new[] { "Readiness" }))
                ;
        }
    }
}
