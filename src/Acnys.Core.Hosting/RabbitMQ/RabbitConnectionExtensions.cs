using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public static class RabbitConnectionExtensions
    {
        public static EventingBasicConsumer CreateConsumer(this IConnection connection, string queue, bool autoAck, string consumerTag = null, Dictionary<string, object> arguments = null, Action<RabbitConsumerFactory> factory = null)
        {
            var fact = new RabbitConsumerFactory();
            fact.UseConnection(() => connection);
            factory?.Invoke(fact);
            return fact.CreateConsumer(queue, autoAck, consumerTag, arguments);
        }

        //public static IConnection CreateBinding(this IConnection connection)
        //{
        //    using var channel = connection.CreateModel();

        //    return connection;

        //}
    }
}
