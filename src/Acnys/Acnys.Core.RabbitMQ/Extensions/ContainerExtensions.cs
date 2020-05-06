using System;
using Acnys.Core.Eventing.Abstractions;
using Autofac;
using RabbitMQ.Client;
using Serilog;

namespace Acnys.Core.RabbitMQ.Extensions
{
    public static class ContainerExtensions
    {
        public static ContainerBuilder AddRabbitConnection(this ContainerBuilder builder, Action<ConnectionFactory> connectionBuilder, object serviceKey = null)
        {
            Log.Debug("Adding RabbitMQ connection with service key {serviceKey}", serviceKey);

            var connectionFactory = new ConnectionFactory();
            connectionBuilder(connectionFactory);
            var connection = connectionFactory.CreateConnection();

            if (serviceKey == null)
            {
                builder.RegisterInstance(connection).As<IConnection>();

            }
            else
            {
                builder.RegisterInstance(connection).Keyed<IConnection>(serviceKey);
            }

            return builder;
        }

        public static ContainerBuilder AddRabbitEventListener(this ContainerBuilder builder, string queue, bool requeueOnNack = false)
        {
            Log.Debug("Adding RabbitMQ event listener for queue {queue}", queue);

            builder.Register(context =>
                    new RabbitEventListener(
                        context.Resolve<ILogger>().ForContext<RabbitEventListener>(),
                        context.Resolve<IConnection>(),
                        context.Resolve<ILifetimeScope>(),
                        queue,
                        string.Empty,
                        requeueOnNack,
                        null,
                        RabbitEventListener.Default
                    )
                ).AsSelf().SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddRabbitEventPublisher(this ContainerBuilder builder, string exchange)
        {
            Log.Debug("Adding RabbitMQ event publisher for exchange {queue}", exchange);

            builder.Register(context => new RabbitEventPublisher(
                    context.Resolve<ILogger>().ForContext<RabbitEventPublisher>(),
                    context.Resolve<IConnection>(),
                    exchange,
                    RabbitEventPublisher.DefaultContextBuilder
                    )
            ).AsImplementedInterfaces().SingleInstance();

            return builder;
        }

        public static ContainerBuilder AutoStartRabbitEventListeners(this ContainerBuilder builder)
        {
            builder.RegisterType<RabbitAutoStartService>().AsImplementedInterfaces().SingleInstance();
            return builder;
        }
    }
}
