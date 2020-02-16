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

        public static ContainerBuilder AddEventListener(this ContainerBuilder builder, string queue)
        {
            builder.Register(context =>
                    new EventListener(
                        context.Resolve<ILogger>().ForContext<EventListener>(),
                        context.Resolve<IConnection>(),
                        context.Resolve<IDispatchEvent>(),
                        queue,
                        string.Empty,
                        null,
                        EventListener.Default
                    )
                ).AsSelf().SingleInstance().AutoActivate();

            return builder;
        }
    }
}
