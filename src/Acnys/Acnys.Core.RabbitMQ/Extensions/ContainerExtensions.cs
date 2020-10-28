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
            //if (serviceKey != null) connectionFactory.ClientProperties.Add("serviceKey", serviceKey);
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

        public static ContainerBuilder AddRabbitChannel(this ContainerBuilder builder, string channelKey)
        {
            builder.Register(context => context.ResolveKeyed<IConnection>(channelKey).CreateModel())
                .InstancePerLifetimeScope().Keyed<IModel>(channelKey);

            return builder;
        }

        public static ContainerBuilder AddRabbitChannel(this ContainerBuilder builder)
        {
            builder.Register(context => context.Resolve<IConnection>().CreateModel())
                .InstancePerLifetimeScope().As<IModel>();

            return builder;
        }

        public static ContainerBuilder AddRabbitEventListener(this ContainerBuilder builder, string queue, bool requeueOnNack = false, int consumerCount = 1, string consumerTag = "", string connectionKey = null)
        {
            Log.Debug("Adding RabbitMQ event listener for queue {queue}", queue);

            builder.Register(context =>
                    new RabbitEventListener(
                        context.Resolve<ILogger>().ForContext<RabbitEventListener>(),
                        connectionKey == null ? context.Resolve<IConnection>() : context.ResolveKeyed<IConnection>(connectionKey),
                        context.Resolve<ILifetimeScope>(),
                        queue,
                        consumerTag,
                        consumerCount,
                        requeueOnNack,
                        null,
                        RabbitEventListener.Default
                    )
                ).AsSelf().SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddRabbitEventPublisher(this ContainerBuilder builder, string exchange, string connectionKey = null)
        {
            Log.Debug("Adding RabbitMQ event publisher for exchange {queue}", exchange);

            builder.Register(context => new RabbitEventPublisher(
                    context.Resolve<ILogger>().ForContext<RabbitEventPublisher>(),
                    connectionKey == null ? context.Resolve<IModel>() : context.ResolveKeyed<IModel>(connectionKey),
                    exchange,
                    RabbitEventPublisher.DefaultContextBuilder
                    )
            ).AsImplementedInterfaces().InstancePerLifetimeScope();

            return builder;
        }

        public static ContainerBuilder AutoStartRabbitEventListeners(this ContainerBuilder builder)
        {
            builder.RegisterType<RabbitAutoStartService>().AsImplementedInterfaces().SingleInstance();
            return builder;
        }
    }
}
