using System;
using Autofac;
using RabbitMQ.Client;

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
    }
}
